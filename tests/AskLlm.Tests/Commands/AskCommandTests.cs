using System;
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
            Query = "Hi there",
            Model = "gpt-test"
        });

        result.ShouldBe(0);
        await service.Received(1).SendChatRequestAsync(Arg.Is<ChatRequest>(r => r.Message == "Hi there" && r.Model == "gpt-test"), Arg.Any<CancellationToken>());
        console.Received(1).Write(Arg.Any<IRenderable>());
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
            Query = "Question",
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
            Query = "Question",
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
            Query = "Question",
            Model = "gpt-test"
        });

        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("", "gpt-test")]
    [InlineData("   ", "gpt-test")]
    [InlineData("Question", "")]
    [InlineData("Question", "   ")]
    public async Task ExecuteAsync_ReturnsOne_WhenValidationFails(string query, string model)
    {
        var service = Substitute.For<IChatEndpointService>();
        service.IsConfigured.Returns(true);

        var command = CreateCommand(service, Substitute.For<IAnsiConsole>());

        var result = await command.ExecuteAsync(null!, new AskCommandSettings
        {
            Query = query,
            Model = model
        });

        result.ShouldBe(1);
        await service.DidNotReceive().SendChatRequestAsync(Arg.Any<ChatRequest>(), Arg.Any<CancellationToken>());
    }
}
