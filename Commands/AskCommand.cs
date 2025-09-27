using AskLlm;
using AskLlm.Models;
using AskLlm.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using static Crayon.Output;

namespace AskLlm.Commands;

public sealed class AskCommand
{
    private readonly IChatEndpointService _chatEndpointService;
    private readonly ILogger<AskCommand> _logger;

    public AskCommand(IChatEndpointService chatEndpointService, ILogger<AskCommand> logger)
    {
        _chatEndpointService = chatEndpointService;
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

        if (!TryResolveColor(settings.Color, out var responseColor, out var colorError))
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
            Console.WriteLine("Requesting response from the LLM...");

            var response = await _chatEndpointService.SendChatRequestAsync(request, cancellationToken);

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
                RenderSuccess(response, responseColor);
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
            Console.WriteLine(Bright.Green($"Response written to {settings.OutputFile}"));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write output file {File}", settings.OutputFile);
            RenderError("Unable to write the response to the specified output file.");
            return false;
        }
    }

    private void RenderSuccess(ChatResponse response, ConsoleColor? color)
    {
        var header = $"Response from {response.Model}";
        var separatorLength = Math.Max(header.Length, 24);
        var separator = new string('=', separatorLength);

        Console.WriteLine();
        Console.WriteLine(Bright.Green(separator));
        Console.WriteLine(Bright.Green(header));
        Console.WriteLine(Bright.Green(separator));
        Console.WriteLine();

        var content = response.Content ?? string.Empty;
        Console.WriteLine(ApplyColor(content, color));
        Console.WriteLine();
    }

    private void RenderError(string message)
    {
        Console.Error.WriteLine(Bright.Red($"Error: {message}"));
    }

    private bool TryStoreCommandDefaults(AskCommandSettings settings)
    {
        try
        {
            var defaults = BuildDefaultsString(settings);
            Environment.SetEnvironmentVariable(EnvironmentVariableNames.Defaults, string.IsNullOrWhiteSpace(defaults) ? null : defaults);

            if (!string.IsNullOrWhiteSpace(defaults))
            {
                Console.WriteLine(Bright.Green($"Stored command defaults in {EnvironmentVariableNames.Defaults}."));
            }
            else
            {
                Console.WriteLine(Bright.Green("Cleared stored command defaults."));
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

    private static bool TryResolveColor(string? colorValue, out ConsoleColor? color, out string? error)
    {
        color = null;
        error = null;

        if (string.IsNullOrWhiteSpace(colorValue))
        {
            return true;
        }

        var trimmed = colorValue.Trim();

        if (Enum.TryParse(trimmed, ignoreCase: true, out ConsoleColor parsed))
        {
            color = parsed;
            return true;
        }

        error = "The value provided for --color is not a valid console color name.";
        return false;
    }

    private static string ApplyColor(string text, ConsoleColor? color)
    {
        if (color is null)
        {
            return text;
        }

        return color.Value switch
        {
            ConsoleColor.Black => Black(text),
            ConsoleColor.DarkBlue => Blue(text),
            ConsoleColor.DarkGreen => Green(text),
            ConsoleColor.DarkCyan => Cyan(text),
            ConsoleColor.DarkRed => Red(text),
            ConsoleColor.DarkMagenta => Magenta(text),
            ConsoleColor.DarkYellow => Yellow(text),
            ConsoleColor.DarkGray => Bright.Black(text),
            ConsoleColor.Blue => Bright.Blue(text),
            ConsoleColor.Green => Bright.Green(text),
            ConsoleColor.Cyan => Bright.Cyan(text),
            ConsoleColor.Red => Bright.Red(text),
            ConsoleColor.Magenta => Bright.Magenta(text),
            ConsoleColor.Yellow => Bright.Yellow(text),
            ConsoleColor.Gray => Bright.White(text),
            ConsoleColor.White => Bright.White(text),
            _ => text
        };
    }
}
