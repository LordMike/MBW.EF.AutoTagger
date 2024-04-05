using System;
using Microsoft.Extensions.Logging;

namespace MBW.EF.AutoTagger.Tests.TestUtilities;

internal class MemoryLogger : ILogger
{
    private readonly MemoryLoggerProvider.LogMessage _onLog;

    public MemoryLogger(MemoryLoggerProvider.LogMessage onLog)
    {
        _onLog = onLog;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _onLog( eventId, formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoopDisposable.Instance;

    private class NoopDisposable : IDisposable
    {
        internal static NoopDisposable Instance { get; } = new();

        private NoopDisposable()
        {
        }

        public void Dispose()
        {
        }
    }
}