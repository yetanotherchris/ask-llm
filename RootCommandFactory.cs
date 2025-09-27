using AskLlm.Commands;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace AskLlm;

public sealed class RootCommandFactory
{
    private readonly AskCommand _askCommand;
    private readonly IAnsiConsole _console;

    public RootCommandFactory(AskCommand askCommand, IAnsiConsole console)
    {
        _askCommand = askCommand;
        _console = console;
    }

    public RootCommand Create(string applicationVersion)
    {
        var modelOption = new Option<string?>("--model", "The model identifier to send the request to.")
        {
            ArgumentHelpName = "model_name"
        };
        var promptOption = new Option<string?>("--prompt", "The prompt text to send to the model.")
        {
            ArgumentHelpName = "prompt"
        };
        var inputFileOption = new Option<string?>("--input-file", "Optional file path that supplies the prompt text.")
        {
            ArgumentHelpName = "path"
        };
        var outputFileOption = new Option<string?>("--output-file", "Optional file path to write the response to.")
        {
            ArgumentHelpName = "path"
        };
        var storeDefaultsOption = new Option<bool>("--store", "Store provided options (excluding --prompt) for future runs.");
        var versionOption = new Option<bool>("--version", "Show the application version.");

        var rootCommand = new RootCommand("Send a prompt to an LLM provider.")
        {
            modelOption,
            promptOption,
            inputFileOption,
            outputFileOption,
            storeDefaultsOption
        };

        rootCommand.AddAlias("ask");
        rootCommand.AddOption(versionOption);

        rootCommand.SetHandler(async (InvocationContext context) =>
        {
            var showVersion = context.ParseResult.GetValueForOption(versionOption);
            if (showVersion)
            {
                _console.MarkupLine($"askllm v{Markup.Escape(applicationVersion)}");
                context.ExitCode = 0;
                return;
            }

            if (!HasUserProvidedInput(context.ParseResult))
            {
                var helpContext = new HelpContext(context.HelpBuilder, rootCommand, System.Console.Out, context.ParseResult);
                context.HelpBuilder.Write(helpContext);
                context.ExitCode = 0;
                return;
            }

            var settings = new AskCommandSettings
            {
                Model = (context.ParseResult.GetValueForOption(modelOption) ?? string.Empty).Trim(),
                Prompt = context.ParseResult.GetValueForOption(promptOption) ?? string.Empty,
                InputFile = context.ParseResult.GetValueForOption(inputFileOption),
                OutputFile = context.ParseResult.GetValueForOption(outputFileOption),
                StoreDefaults = context.ParseResult.GetValueForOption(storeDefaultsOption)
            };

            var exitCode = await _askCommand.ExecuteAsync(settings, context.GetCancellationToken()).ConfigureAwait(false);
            context.ExitCode = exitCode;
        });

        return rootCommand;
    }

    private static bool HasUserProvidedInput(ParseResult parseResult)
    {
        return parseResult.Tokens.Count > 0 || parseResult.UnmatchedTokens.Count > 0;
    }
}
