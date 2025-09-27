using System;
using System.Threading.Tasks;
using AskLlm.Models;
using AskLlm.Services;
using AskLlm.Tests.Support;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace AskLlm.Tests.Services;

public sealed class ChatEndpointServiceTests : IDisposable
{
    private readonly string? _originalApiKey;
    private readonly string? _originalEndpoint;

    public ChatEndpointServiceTests()
    {
        _originalApiKey = Environment.GetEnvironmentVariable("ASKLLM_API_KEY");
        _originalEndpoint = Environment.GetEnvironmentVariable("ASKLLM_API_ENDPOINT");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("ASKLLM_API_KEY", _originalApiKey);
        Environment.SetEnvironmentVariable("ASKLLM_API_ENDPOINT", _originalEndpoint);
    }

    [Fact]
    public void IsConfigured_ReturnsFalse_WhenApiKeyMissing()
    {
        var service = CreateService(null, AskLlmSettings.DefaultApiEndpoint);
        service.IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public void IsConfigured_ReturnsTrue_WithValidConfiguration()
    {
        var service = CreateService("key", AskLlmSettings.DefaultApiEndpoint);
        service.IsConfigured.ShouldBeTrue();
    }

    [Fact]
    public async Task SendChatRequestAsync_ReturnsFailure_WhenNotConfigured()
    {
        var service = CreateService(string.Empty, AskLlmSettings.DefaultApiEndpoint);
        var result = await service.SendChatRequestAsync(new ChatRequest("hello", "gpt-test"));

        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
    }

    [Fact]
    public async Task SendChatRequestAsync_Throws_WhenRequestIsNull()
    {
        var service = CreateService("key", AskLlmSettings.DefaultApiEndpoint);

        await Should.ThrowAsync<ArgumentNullException>(() => service.SendChatRequestAsync(null!));
    }

    [Fact]
    public async Task SendChatRequestAsync_Throws_WhenMessageIsEmpty()
    {
        var service = CreateService("key", AskLlmSettings.DefaultApiEndpoint);

        await Should.ThrowAsync<ArgumentException>(() => service.SendChatRequestAsync(new ChatRequest(string.Empty, "model")));
    }

    [Fact]
    public async Task SendChatRequestAsync_Throws_WhenModelIsEmpty()
    {
        var service = CreateService("key", AskLlmSettings.DefaultApiEndpoint);

        await Should.ThrowAsync<ArgumentException>(() => service.SendChatRequestAsync(new ChatRequest("hello", string.Empty)));
    }

    private static ChatEndpointService CreateService(string? key, string? endpoint)
    {
        Environment.SetEnvironmentVariable("ASKLLM_API_KEY", key);
        Environment.SetEnvironmentVariable("ASKLLM_API_ENDPOINT", endpoint);

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var settings = AskLlmSettings.Create(configuration);
        return new ChatEndpointService(settings, TestLoggerFactory.CreateLogger<ChatEndpointService>());
    }
}
