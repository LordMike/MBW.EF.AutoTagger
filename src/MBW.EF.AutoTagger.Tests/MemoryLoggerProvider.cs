using Microsoft.Extensions.Logging;

namespace MBW.EF.AutoTagger.Tests;

internal class MemoryLoggerProvider : ILoggerProvider
{
    private readonly LogMessage _onLog;

    public delegate void LogMessage(string category, LogLevel level, EventId eventId, string message);

    public MemoryLoggerProvider(LogMessage onLog)
    {
        _onLog = onLog;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName) => new MemoryLogger(_onLog, categoryName);
}