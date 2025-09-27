using System;
using Microsoft.Extensions.Configuration;
using AskLlm;

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

    public static AskLlmSettings Create(IConfiguration configuration)
    {
        var apiKey = configuration[EnvironmentVariableNames.ApiKey] ?? string.Empty;
        var endpoint = configuration[EnvironmentVariableNames.ApiEndpoint];

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = DefaultApiEndpoint;
        }

        return new AskLlmSettings(apiKey.Trim(), endpoint.Trim());
    }
}
