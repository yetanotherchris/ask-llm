using AskLlm.Models;
using AskLlm.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace AskLlm.Commands;

public class AskCommandSettings : CommandSettings
{
    [CommandOption("--prompt")]
    [Description("The prompt to send to the LLM")]
    public string Prompt { get; init; } = string.Empty;

    [CommandOption("--model")]
    [Description("The model to use with the LLM.")]
    public string Model { get; init; } = string.Empty;

    [CommandOption("--input-file")]
    [Description("Optional file to use as the prompt.")]
    public string? InputFile { get; init; }

    [CommandOption("--output-file")]
    [Description("Optional file to write the LLM response to.")]
    public string? OutputFile { get; init; }

    public override ValidationResult Validate()
    {
        var hasPrompt = !string.IsNullOrWhiteSpace(Prompt);
        var hasInputFile = !string.IsNullOrWhiteSpace(InputFile);

        if (!hasPrompt && !hasInputFile)
        {
            return ValidationResult.Error("A prompt must be provided or an input file must be specified using --input-file.");
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

public class AskCommand : AsyncCommand<AskCommandSettings>
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

        var (requestMessage, resolveError) = await GetPromptTextAsync(settings);
        if (resolveError is not null)
        {
            RenderError(resolveError);
            return 1;
        }

        var request = new ChatRequest(requestMessage!, settings.Model.Trim());

        try
        {
            var response = await _chatEndpointService.SendChatRequestAsync(request, CancellationToken.None);

            if (!response.Success)
            {
                var responseError = response.ErrorMessage ?? "The model returned an unsuccessful response.";
                RenderError(responseError);
                return 1;
            }

            if (!await TryWriteOutputFileAsync(settings, response.Content))
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

        return (settings.Prompt.Trim(), null);
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

            await File.WriteAllTextAsync(settings.OutputFile, content);
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

        _console.Write(Markup.Escape(response.Content));
    }

    private void RenderError(string message)
    {
        _console.MarkupLine($"[red]Error:[/] {Markup.Escape(message)}");
    }
}
