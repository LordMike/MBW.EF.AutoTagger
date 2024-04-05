using Microsoft.Extensions.Logging;

namespace MBW.EF.AutoTagger.Tests.TestUtilities;

internal record LogEntry(EventId EventId, string Message);