using System;
using System.IO;
using System.Threading;
using AskLlm.Models;
using AskLlm.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AskLlm.Commands;

public sealed class AskCommandSettings : CommandSettings
{
    [CommandArgument(0, "[query]")]
    public string Query { get; init; } = string.Empty;

    [CommandOption("-m|--model")]
    public string Model { get; init; } = string.Empty;

    [CommandOption("--input-file")]
    public string? InputFile { get; init; }

    [CommandOption("--output-file")]
    public string? OutputFile { get; init; }

    public override ValidationResult Validate()
    {
        var hasQuery = !string.IsNullOrWhiteSpace(Query);
        var hasInputFile = !string.IsNullOrWhiteSpace(InputFile);

        if (!hasQuery && !hasInputFile)
        {
            return ValidationResult.Error("A query must be provided or an input file must be specified using --input-file.");
        }

        if (hasInputFile && !File.Exists(InputFile))
        {
            return ValidationResult.Error("The file specified by --input-file does not exist.");
        }

        if (string.IsNullOrWhiteSpace(Model))
        {
            return ValidationResult.Error("A model must be specified using --model.");
        }

        return ValidationResult.Success();
    }
}

public sealed class AskCommand : AsyncCommand<AskCommandSettings>
{
    private readonly IChatEndpointService _chatEndpointService;
    private readonly IAnsiConsole _console;
    private readonly ILogger<AskCommand> _logger;

    public AskCommand(IChatEndpointService chatEndpointService, IAnsiConsole console, ILogger<AskCommand> logger)
    {
        _chatEndpointService = chatEndpointService;
        _console = console;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AskCommandSettings settings)
    {
        var validation = settings.Validate();
        if (!validation.Successful)
        {
            RenderError(validation.Message ?? "Invalid command input.");
            return 1;
        }

        if (!_chatEndpointService.IsConfigured)
        {
            const string configurationError = "The ASKLLM_API_KEY environment variable is not configured.";
            RenderError(configurationError);
            return 1;
        }

        var (requestMessage, resolveError) = await GetPromptTextAsync(settings).ConfigureAwait(false);
        if (resolveError is not null)
        {
            RenderError(resolveError);
            return 1;
        }

        var request = new ChatRequest(requestMessage!, settings.Model.Trim());

        try
        {
            var response = await _chatEndpointService.SendChatRequestAsync(
                    request,
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (!response.Success)
            {
                var responseError = response.ErrorMessage ?? "The model returned an unsuccessful response.";
                RenderError(responseError);
                return 1;
            }

            if (!await TryWriteOutputFileAsync(settings, response.Content).ConfigureAwait(false))
            {
                return 1;
            }

            if (string.IsNullOrWhiteSpace(settings.OutputFile))
            {
                RenderSuccess(response);
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing ask command.");
            RenderError("An unexpected error occurred. Please try again.");
            return 1;
        }
    }

    private async Task<(string? Message, string? Error)> GetPromptTextAsync(AskCommandSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.InputFile))
        {
            try
            {
                var content = await File.ReadAllTextAsync(settings.InputFile).ConfigureAwait(false);
                return (content, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read input file {File}", settings.InputFile);
                return (null, "Unable to read the input file specified by --input-file.");
            }
        }

        return (settings.Query.Trim(), null);
    }

    private async Task<bool> TryWriteOutputFileAsync(AskCommandSettings settings, string content)
    {
        if (string.IsNullOrWhiteSpace(settings.OutputFile))
        {
            return true;
        }

        try
        {
            var directory = Path.GetDirectoryName(settings.OutputFile);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(settings.OutputFile, content).ConfigureAwait(false);
            _console.MarkupLine($"[green]Response written to {Markup.Escape(settings.OutputFile)}[/]");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write output file {File}", settings.OutputFile);
            RenderError("Unable to write the response to the specified output file.");
            return false;
        }
    }

    private void RenderSuccess(ChatResponse response)
    {
        var panel = new Panel(new Markup(Markup.Escape(response.Content)))
            .Header(new PanelHeader($"Response from {Markup.Escape(response.Model)}"))
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Green));

        _console.Write(panel);
    }

    private void RenderError(string message)
    {
        _console.MarkupLine($"[red]Error:[/] {Markup.Escape(message)}");
    }
}

internal sealed class AskCommandProxy : AsyncCommand<AskCommandSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, AskCommandSettings settings)
    {
        if (context is null)
        {
            throw new InvalidOperationException("The command context must be provided.");
        }

        var command = context.Data as AskCommand ?? AskCommandEntryPoint.Resolve();
        return command.ExecuteAsync(context, settings);
    }
}

internal static class AskCommandEntryPoint
{
    private static AskCommand? _command;

    public static void Configure(AskCommand command)
    {
        _command = command ?? throw new ArgumentNullException(nameof(command));
    }

    public static AskCommand Resolve()
    {
        return _command ?? throw new InvalidOperationException("The AskCommand has not been configured.");
    }
}
