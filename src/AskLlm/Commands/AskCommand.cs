using System;
using System.Threading;
using AskLlm.Models;
using AskLlm.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AskLlm.Commands;

public sealed class AskCommandSettings : CommandSettings
{
    [CommandArgument(0, "<query>")]
    public string Query { get; init; } = string.Empty;

    [CommandOption("-m|--model")]
    public string Model { get; init; } = string.Empty;

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            return ValidationResult.Error("The query must not be empty.");
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
            const string message = "The ASKLLM_API_KEY environment variable is not configured.";
            RenderError(message);
            return 1;
        }

        var request = new ChatRequest(settings.Query.Trim(), settings.Model.Trim());

        try
        {
            var response = await _chatEndpointService.SendChatRequestAsync(
                    request,
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (!response.Success)
            {
                var error = response.ErrorMessage ?? "The model returned an unsuccessful response.";
                RenderError(error);
                return 1;
            }

            RenderSuccess(response);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing ask command.");
            RenderError("An unexpected error occurred. Please try again.");
            return 1;
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
