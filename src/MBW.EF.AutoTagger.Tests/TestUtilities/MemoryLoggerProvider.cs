using Microsoft.Extensions.Logging;

namespace MBW.EF.AutoTagger.Tests.TestUtilities;

internal class MemoryLoggerProvider : ILoggerProvider
{
    private readonly LogMessage _onLog;

    public delegate void LogMessage(EventId eventId, string message);

    public MemoryLoggerProvider(LogMessage onLog)
    {
        _onLog = onLog;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName) => new MemoryLogger(_onLog);
}