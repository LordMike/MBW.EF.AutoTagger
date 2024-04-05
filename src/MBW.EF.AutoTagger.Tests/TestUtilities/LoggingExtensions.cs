using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MBW.EF.AutoTagger.Tests.TestUtilities;

internal static class LoggingExtensions
{
    public static ILoggingBuilder AddMemoryLogging(this ILoggingBuilder builder, IList<LogEntry> list)
    {
        return builder.AddProvider(new MemoryLoggerProvider((eventId, message) => list.Add(new LogEntry(eventId, message))));
    }
}