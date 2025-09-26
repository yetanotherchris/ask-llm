using Microsoft.Extensions.Logging;

namespace AskLlm.Tests.Support;

public static class TestLoggerFactory
{
    public static ILogger<T> CreateLogger<T>()
    {
        return LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Trace)).CreateLogger<T>();
    }
}
