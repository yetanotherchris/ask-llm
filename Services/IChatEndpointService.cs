using AskLlm.Models;

namespace AskLlm.Services;

public interface IChatEndpointService
{
    bool IsConfigured { get; }

    Task<ChatResponse> SendChatRequestAsync(ChatRequest request, Action<string>? onToken = null, CancellationToken cancellationToken = default);
}
