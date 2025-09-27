namespace AskLlm.Commands;

public readonly record struct CommandValidationResult(bool Successful, string? Message)
{
    public static CommandValidationResult Success() => new(true, null);

    public static CommandValidationResult Error(string message) => new(false, message);
}
