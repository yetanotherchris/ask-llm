using AskLlm.Commands;
using AskLlm.IoC;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AskLlm;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddAskLlmServices();

        using var provider = services.BuildServiceProvider();
        var askCommand = provider.GetRequiredService<AskCommand>();
        AskCommandEntryPoint.Configure(askCommand);

        var app = new CommandApp<AskCommandProxy>();

        app.Configure(config =>
        {
            config.SetApplicationName("askllm");
            config.AddCommand<AskCommandProxy>("ask")
                .WithDescription("Send a query to a configured large language model.")
                .WithExample(new[] { "askllm", "\"Hello there\"", "--model", "gpt-4o-mini" })
                .WithData(askCommand);
            config.ValidateExamples();
        });

        return await app.RunAsync(args).ConfigureAwait(false);
    }
}
