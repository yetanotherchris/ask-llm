using System.IO;

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
