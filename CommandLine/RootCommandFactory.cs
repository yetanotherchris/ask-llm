using AskLlm.Commands;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace AskLlm.CommandLine
{
    public sealed class RootCommandFactory
    {
        private readonly AskCommand _askCommand;

        public RootCommandFactory(AskCommand askCommand)
        {
            _askCommand = askCommand;
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
            var colorOption = new Option<string?>("--color", "Optional console color name used when rendering responses.")
            {
                ArgumentHelpName = "color"
            };
            var storeDefaultsOption = new Option<bool>("--store", "Store provided options (excluding --prompt) for future runs.");

            var rootCommand = new RootCommand("Send a prompt to an LLM provider.")
            {
                modelOption,
                promptOption,
                inputFileOption,
                outputFileOption,
                storeDefaultsOption,
                colorOption
            };

            rootCommand.AddAlias("ask");

            rootCommand.SetHandler(async (context) =>
            {
                if (!HasUserProvidedInput(context.ParseResult))
                {
                    var helpContext = new HelpContext(context.HelpBuilder, rootCommand, Console.Out, context.ParseResult);
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
                    Color = context.ParseResult.GetValueForOption(colorOption),
                    StoreDefaults = context.ParseResult.GetValueForOption(storeDefaultsOption)
                };

                var exitCode = await _askCommand.ExecuteAsync(settings, context.GetCancellationToken());
                context.ExitCode = exitCode;
            });

            return rootCommand;
        }

        private static bool HasUserProvidedInput(ParseResult parseResult)
        {
            return parseResult.Tokens.Count > 0 || parseResult.UnmatchedTokens.Count > 0;
        }
    }
}