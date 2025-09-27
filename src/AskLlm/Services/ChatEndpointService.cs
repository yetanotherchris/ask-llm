using System.Text;
using AskLlm.Models;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace AskLlm.Services;

public class ChatEndpointService : IChatEndpointService
{
    public bool IsConfigured => _settings.IsValid;

    private readonly AskLlmSettings _settings;
    private readonly ILogger<ChatEndpointService> _logger;

    public ChatEndpointService(AskLlmSettings settings, ILogger<ChatEndpointService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<ChatResponse> SendChatRequestAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("The message must not be empty.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new ArgumentException("The model must not be empty.", nameof(request));
        }

        if (!IsConfigured)
        {
            const string message = "The chat endpoint service is not configured.";
            _logger.LogError(message);
            return new ChatResponse(string.Empty, request.Model, false, message);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var credential = new ApiKeyCredential(_settings.ApiKey);
            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = new Uri(_settings.ApiEndpoint)
            };

            var chatClient = new ChatClient(request.Model, credential, clientOptions);

            var completionResult = await chatClient.CompleteChatAsync(
                new ChatMessage[]
                {
                    new UserChatMessage(request.Message)
                });

            var completion = completionResult.Value;

            if (completion?.Content is null)
            {
                const string missingContentMessage = "The model did not return any content.";
                _logger.LogWarning(missingContentMessage);
                return new ChatResponse(string.Empty, request.Model, false, missingContentMessage);
            }

            var responseText = ExtractTextFromContent(completion.Content).Trim();

            if (string.IsNullOrEmpty(responseText))
            {
                const string emptyMessage = "The model did not return any content.";
                _logger.LogWarning(emptyMessage);
                return new ChatResponse(string.Empty, request.Model, false, emptyMessage);
            }

            return new ChatResponse(responseText, request.Model, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send chat request.");
            return new ChatResponse(string.Empty, request.Model, false, ex.Message);
        }
    }

    private static string ExtractTextFromContent(ChatMessageContent content)
    {
        var builder = new StringBuilder();

        foreach (var part in content)
        {
            if (part.Kind == ChatMessageContentPartKind.Text && !string.IsNullOrWhiteSpace(part.Text))
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(part.Text);
            }
        }

        return builder.ToString();
    }
}
