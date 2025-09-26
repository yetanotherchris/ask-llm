namespace AskLlm.Models;

public sealed record ChatResponse(string Content, string Model, bool Success, string? ErrorMessage = null);
