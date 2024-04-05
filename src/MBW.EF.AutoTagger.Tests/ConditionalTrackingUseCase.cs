using System;
using System.Collections.Generic;
using System.Linq;
using MBW.EF.AutoTagger.Database;
using MBW.EF.AutoTagger.Extensions;
using MBW.EF.AutoTagger.Tests.TestContext;
using MBW.EF.AutoTagger.Tests.TestUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MBW.EF.AutoTagger.Tests;

public class ConditionalTrackingUseCase
{
    private const int CommandExecuted = 20101;

    [Fact]
    public void TestSimpleQuery()
    {
        bool initialRun = true;
        using var context = new TestDbContext(config =>
        {
            // Filter out xunit frames
            config.FrameFilter = (assembly, _) => !assembly.Name.StartsWith("xunit.", StringComparison.Ordinal);
            config.TagFilter = (context, command) =>
            {
                if (initialRun || context == null)
                    return false;

                var scopedHolder = context.Database.GetService<ScopedFilteringHolder>();
                Assert.True(scopedHolder.ShouldBeTagged.HasValue);

                return scopedHolder.ShouldBeTagged.Value;
            };
        }, services => services.AddScoped<ScopedFilteringHolder>());

        // TestDbContext does some setup which will invoke the DB. But we can't set our scoped holder just yet.
        // This is a quick workaround
        initialRun = false;

        using (IServiceScope scopeNotTagged = context.CreateScope())
        {
            // Set this scope to be not tagged
            scopeNotTagged.ServiceProvider.GetRequiredService<ScopedFilteringHolder>()
                .ShouldBeTagged = false;

            Context db = scopeNotTagged.ServiceProvider.GetRequiredService<Context>();
            Assert.Single(db.BlogPosts.Where(s => s.Id == 1));

            LogEntry log = Assert.Single(context.Logs, log => log.EventId.Id == CommandExecuted);
            Assert.DoesNotContain(nameof(TestSimpleQuery), log.Message);
        }

        context.Reset();

        using (IServiceScope scopeTagged = context.CreateScope())
        {
            // Set this scop to be tagged
            scopeTagged.ServiceProvider.GetRequiredService<ScopedFilteringHolder>()
                .ShouldBeTagged = true;

            Context db = scopeTagged.ServiceProvider.GetRequiredService<Context>();
            Assert.Single(db.BlogPosts.Where(s => s.Id == 1));

            LogEntry log = Assert.Single(context.Logs, log => log.EventId.Id == CommandExecuted);
            Assert.Contains(nameof(TestSimpleQuery), log.Message);
        }
    }

    /// <summary>
    /// This could represent an HttpContext or some other scoped service.
    /// </summary>
    class ScopedFilteringHolder
    {
        public bool? ShouldBeTagged { get; set; }
    }
}