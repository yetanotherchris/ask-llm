using System;
using Microsoft.Extensions.Configuration;

namespace AskLlm.Models;

public class AskLlmSettings
{
    public const string DefaultApiEndpoint = "https://openrouter.ai/api/v1";

    public string ApiKey { get; }

    public string ApiEndpoint { get; }

    public bool IsValid => !string.IsNullOrWhiteSpace(ApiKey) && Uri.TryCreate(ApiEndpoint, UriKind.Absolute, out _);

    private AskLlmSettings(string apiKey, string apiEndpoint)
    {
        ApiKey = apiKey;
        ApiEndpoint = apiEndpoint;
    }

    public static AskLlmSettings LoadFromEnvironment(IConfiguration? configuration = null)
    {
        configuration ??= new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var apiKey = configuration["ASKLLM_API_KEY"] ?? string.Empty;
        var endpoint = configuration["ASKLLM_API_ENDPOINT"];

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = DefaultApiEndpoint;
        }

        return new AskLlmSettings(apiKey.Trim(), endpoint.Trim());
    }
}
