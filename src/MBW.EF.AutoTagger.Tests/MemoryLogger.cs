using System;
using Microsoft.Extensions.Logging;

namespace MBW.EF.AutoTagger.Tests;

internal class MemoryLogger : ILogger
{
    private readonly MemoryLoggerProvider.LogMessage _onLog;
    private readonly string _categoryName;

    public MemoryLogger(MemoryLoggerProvider.LogMessage onLog, string categoryName)
    {
        _onLog = onLog;
        _categoryName = categoryName;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _onLog(_categoryName, logLevel, eventId, formatter(state, exception));
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