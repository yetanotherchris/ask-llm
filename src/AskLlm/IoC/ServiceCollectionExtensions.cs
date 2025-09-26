using AskLlm.Models;
using AskLlm.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace AskLlm.IoC;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAskLlmServices(this IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(AskLlmSettings.LoadFromEnvironment(configuration));
        services.AddSingleton<IChatEndpointService, ChatEndpointService>();
        services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));

        return services;
    }
}
