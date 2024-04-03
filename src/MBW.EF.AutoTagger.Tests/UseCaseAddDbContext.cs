using System;
using System.Collections.Generic;
using System.Linq;
using MBW.EF.AutoTagger.Extensions;
using MBW.EF.AutoTagger.Tests.TestContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MBW.EF.AutoTagger.Tests;

public class UseCaseAddDbContext : IDisposable
{
    private record LogEntry(string Category, LogLevel Level, EventId EventId, string Message);
    
    private const int CommandExecuted = 20101;
    
    private readonly SqliteConnection dbConn;
    private readonly ServiceProvider _sp;
    private readonly List<LogEntry> _logs;

    public UseCaseAddDbContext()
    {
        _logs = new List<LogEntry>();

        dbConn = new SqliteConnection("Data Source=:memory:;");
        dbConn.Open();

        // Use case: Using .AddDbContext<>() and configuring the expression validation in that
        _sp = new ServiceCollection()
            .AddLogging(x => x
                .SetMinimumLevel(LogLevel.Information)
                .AddProvider(new MemoryLoggerProvider((category, level, eventId, message) => _logs.Add(new LogEntry(category, level, eventId, message))))
            )
            .AddDbContext<Context>(x => x
                .UseSqlite(dbConn)
                .AddCallsiteTagging()
            )
            .BuildServiceProvider();

        using IServiceScope scope = _sp.CreateScope();
        Context db = scope.ServiceProvider.GetRequiredService<Context>();
        db.Database.EnsureCreated();

        _logs.Clear();
    }

    [Fact]
    public void TestSimpleQuery()
    {
        using IServiceScope scope = _sp.CreateScope();
        Context db = scope.ServiceProvider.GetRequiredService<Context>();

        List<BlogPost> items = db.BlogPosts.Where(s => s.Id == 1).ToList();
        Assert.Single(items);

        LogEntry log = Assert.Single(_logs, log => log.EventId.Id == CommandExecuted);
        Assert.Contains(nameof(TestSimpleQuery), log.Message);
    }

    public void Dispose()
    {
        _sp?.Dispose();
    }
}