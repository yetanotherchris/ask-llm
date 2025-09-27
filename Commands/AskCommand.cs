using AskLlm.Models;
using AskLlm.Services;
using AskLlm;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Text;

namespace AskLlm.Commands;

public sealed class AskCommandSettings
{
    public string Prompt { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string? InputFile { get; set; }

    public string? OutputFile { get; set; }

    public string? Color { get; set; }

    public bool StoreDefaults { get; set; }

    public CommandValidationResult Validate()
    {
        var hasPrompt = !string.IsNullOrWhiteSpace(Prompt);
        var hasInputFile = !string.IsNullOrWhiteSpace(InputFile);

        if (!hasPrompt && !hasInputFile)
        {
            return CommandValidationResult.Error("A prompt must be provided or an input file must be specified using --input-file.");
        }

        if (hasInputFile && !File.Exists(InputFile))
        {
            return CommandValidationResult.Error("The file specified by --input-file does not exist.");
        }

        if (string.IsNullOrWhiteSpace(Model))
        {
            return CommandValidationResult.Error("A model must be specified using --model.");
        }

        return CommandValidationResult.Success();
    }
}

public readonly record struct CommandValidationResult(bool Successful, string? Message)
{
    public static CommandValidationResult Success() => new(true, null);

    public static CommandValidationResult Error(string message) => new(false, message);
}

public sealed class AskCommand
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

    public async Task<int> ExecuteAsync(AskCommandSettings settings, CancellationToken cancellationToken)
    {
        var validation = settings.Validate();
        if (!validation.Successful)
        {
            RenderError(validation.Message ?? "Invalid command input.");
            return 1;
        }

        if (settings.StoreDefaults && !TryStoreCommandDefaults(settings))
        {
            return 1;
        }

        if (!TryResolveColor(settings.Color, out var markupColor, out var colorError))
        {
            RenderError(colorError!);
            return 1;
        }

        if (!_chatEndpointService.IsConfigured)
        {
            RenderError($"The {EnvironmentVariableNames.ApiKey} environment variable is not configured.");
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
            ChatResponse? response = null;

            await _console.Status()
                .StartAsync("Requesting response from the LLM...", async _ =>
                {
                    response = await _chatEndpointService.SendChatRequestAsync(request, cancellationToken);
                });

            if (response is null)
            {
                RenderError("The LLM response could not be retrieved.");
                return 1;
            }

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
                RenderSuccess(response, markupColor);
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

            await File.WriteAllTextAsync(settings.OutputFile, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
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

    private void RenderSuccess(ChatResponse response, string? markupColor)
    {
        var panel = new Panel(new Markup(Markup.Escape(response.Content)))
            .Header(new PanelHeader($"Response from {Markup.Escape(response.Model)}"))
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Green));

        var escapedContent = Markup.Escape(response.Content);
        var output = string.IsNullOrWhiteSpace(markupColor)
            ? escapedContent
            : $"[{markupColor}]{escapedContent}[/]";

        _console.Write(output);
    }

    private void RenderError(string message)
    {
        _console.MarkupLine($"[red]Error:[/] {Markup.Escape(message)}");
    }

    private bool TryStoreCommandDefaults(AskCommandSettings settings)
    {
        try
        {
            var defaults = BuildDefaultsString(settings);
            Environment.SetEnvironmentVariable(EnvironmentVariableNames.Defaults, string.IsNullOrWhiteSpace(defaults) ? null : defaults);

            if (!string.IsNullOrWhiteSpace(defaults))
            {
                _console.MarkupLine($"[green]Stored command defaults in {EnvironmentVariableNames.Defaults}.[/]");
            }
            else
            {
                _console.MarkupLine("[green]Cleared stored command defaults.[/]");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store command defaults.");
            RenderError($"Unable to store the command defaults in the {EnvironmentVariableNames.Defaults} environment variable.");
            return false;
        }
    }

    private static string BuildDefaultsString(AskCommandSettings settings)
    {
        var builder = new StringBuilder();

        AppendOption(builder, "--model", settings.Model);
        AppendOption(builder, "--input-file", settings.InputFile);
        AppendOption(builder, "--output-file", settings.OutputFile);
        AppendOption(builder, "--color", settings.Color);

        return builder.ToString();
    }

    private static void AppendOption(StringBuilder builder, string optionName, string? optionValue)
    {
        if (string.IsNullOrWhiteSpace(optionValue))
        {
            return;
        }

        var trimmedValue = optionValue.Trim();

        if (trimmedValue.Length == 0)
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(optionName);
        builder.Append(' ');
        builder.Append(Quote(trimmedValue));
    }

    private static string Quote(string value)
    {
        var escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }

    private static bool TryResolveColor(string? colorValue, out string? markupColor, out string? error)
    {
        markupColor = null;
        error = null;

        if (string.IsNullOrWhiteSpace(colorValue))
        {
            return true;
        }

        var trimmed = colorValue.Trim();

        try
        {
            _ = new Markup($"[{trimmed}]test[/]");
            markupColor = trimmed;
            return true;
        }
        catch (Exception)
        {
            error = "The value provided for --color is not a valid Spectre.Console color.";
            return false;
        }
    }
}
