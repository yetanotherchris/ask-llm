using System;
using System.IO;
using System.Threading;
using AskLlm.Commands;
using AskLlm.Models;
using AskLlm.Services;
using AskLlm.Tests.Support;
using NSubstitute;
using Shouldly;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace AskLlm.Tests.Commands;

public class AskCommandTests
{
    private static AskCommand CreateCommand(
        IChatEndpointService? service = null,
        IAnsiConsole? console = null)
    {
        service ??= Substitute.For<IChatEndpointService>();
        console ??= Substitute.For<IAnsiConsole>();
        return new AskCommand(service, console, TestLoggerFactory.CreateLogger<AskCommand>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsZero_WhenRequestSucceeds()
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(true);
        service.SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse("Hello", "gpt-test", true));

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(service, console);

        var result = await command.ExecuteAsync(null!, new AskCommandSettings
        {
            Prompt = "Hi there ",
            Model = " gpt-test "
        });

        result.ShouldBe(0);
        await service.Received(1).SendChatRequestAsync(Arg.Is<ChatRequest>(r => r.Message == "Hi there" && r.Model == "gpt-test"), Arg.Any<CancellationToken>());
        console.Received(1).Write(Arg.Any<IRenderable>());
    }

    [Fact]
    public async Task ExecuteAsync_UsesInputFileContent_WhenSpecified()
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(true);
        service.SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse("response", "gpt-test", true));

        var filePath = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(filePath, "File content");

            var command = CreateCommand(service, Substitute.For<IAnsiConsole>());

            var result = await command.ExecuteAsync(null!, new AskCommandSettings
            {
                Prompt = string.Empty,
                Model = "gpt-test",
                InputFile = filePath
            });

            result.ShouldBe(0);
            await service.Received(1).SendChatRequestAsync(
                Arg.Is<ChatRequest>(r => r.Message == "File content" && r.Model == "gpt-test"),
                Arg.Any<CancellationToken>());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WritesResponseToOutputFile_WhenSpecified()
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(true);
        service.SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse("output", "gpt-test", true));

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(service, console);

        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var outputPath = Path.Combine(tempDirectory, "result.txt");

        try
        {
            var result = await command.ExecuteAsync(null!, new AskCommandSettings
            {
                Prompt = "Hello",
                Model = "gpt-test",
                OutputFile = outputPath
            });

            result.ShouldBe(0);
            File.ReadAllText(outputPath).ShouldBe("output");
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenServiceNotConfigured()
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(false);

        var console = Substitute.For<IAnsiConsole>();
        var command = CreateCommand(service, console);

        var result = await command.ExecuteAsync(null!, new AskCommandSettings
        {
            Prompt = "Question",
            Model = "gpt-test"
        });

        result.ShouldBe(1);
        await service.DidNotReceive().SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenServiceReturnsFailure()
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(true);
        service.SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChatResponse(string.Empty, "gpt-test", false, "failure"));

        var command = CreateCommand(service, Substitute.For<IAnsiConsole>());

        var result = await command.ExecuteAsync(null!, new AskCommandSettings
        {
            Prompt = "Question",
            Model = "gpt-test"
        });

        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenExceptionIsThrown()
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(true);
        service.SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<ChatResponse>>(_ => throw new InvalidOperationException("boom"));

        var command = CreateCommand(service, Substitute.For<IAnsiConsole>());

        var result = await command.ExecuteAsync(null!, new AskCommandSettings
        {
            Prompt = "Question",
            Model = "gpt-test"
        });

        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("", "gpt-test")]
    [InlineData("   ", "gpt-test")]
    [InlineData("Question", "")]
    [InlineData("Question", "   ")]
    public async Task ExecuteAsync_ReturnsOne_WhenValidationFails(string prompt, string model)
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(true);

        var command = CreateCommand(service, Substitute.For<IAnsiConsole>());

        var result = await command.ExecuteAsync(null!, new AskCommandSettings
        {
            Prompt = prompt,
            Model = model
        });

        result.ShouldBe(1);
        await service.DidNotReceive().SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOne_WhenInputFileDoesNotExist()
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(true);

        var command = CreateCommand(service, Substitute.For<IAnsiConsole>());

        var result = await command.ExecuteAsync(null!, new AskCommandSettings
        {
            Prompt = string.Empty,
            Model = "gpt-test",
            InputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
        });

        result.ShouldBe(1);
        await service.DidNotReceive().SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>());
    }
}
