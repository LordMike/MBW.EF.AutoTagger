using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MBW.EF.AutoTagger.Database;
using MBW.EF.AutoTagger.Extensions;
using MBW.EF.AutoTagger.Tests.TestContext;
using MBW.EF.AutoTagger.Tests.TestUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MBW.EF.AutoTagger.Tests;

internal class TestDbContext : IDisposable, IAsyncDisposable
{
    private readonly SqliteConnection _dbConn;
    public ServiceProvider Services { get; }
    public List<LogEntry> Logs { get; }

    public TestDbContext(Action<CallSiteTaggerConfig>? configure = null, Action<IServiceCollection>? services = null)
    {
        Logs = new List<LogEntry>();

        _dbConn = new SqliteConnection("Data Source=:memory:;");
        _dbConn.Open();

        var serviceCollection = new ServiceCollection()
            .AddLogging(x => x
                .SetMinimumLevel(LogLevel.Information)
                .AddMemoryLogging(Logs)
            )
            .AddDbContext<Context>(x => x
                .UseSqlite(_dbConn)
                .UseCallsiteTagging(configure));

        services?.Invoke(serviceCollection);

        Services = serviceCollection
            .BuildServiceProvider();

        using IServiceScope scope = Services.CreateScope();
        Context db = scope.ServiceProvider.GetRequiredService<Context>();
        db.Database.EnsureCreated();

        Logs.Clear();
    }

    public IServiceScope CreateScope() => Services.CreateScope();

    public void Dispose()
    {
        _dbConn?.Dispose();
        Services?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbConn != null) await _dbConn.DisposeAsync();
        if (Services != null) await Services.DisposeAsync();
    }

    public void Reset()
    {
        Logs.Clear();
    }
}