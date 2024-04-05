using System.Collections.Generic;
using System.Linq;
using MBW.EF.AutoTagger.Database;
using MBW.EF.AutoTagger.Tests.TestContext;
using MBW.EF.AutoTagger.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MBW.EF.AutoTagger.Tests;

public class SimpleUseCase
{
    private const int CommandExecuted = 20101;

    [Fact]
    public void TestSimpleQuery()
    {
        using TestDbContext context = new TestDbContext();

        using IServiceScope scope = context.CreateScope();
        Context db = scope.ServiceProvider.GetRequiredService<Context>();

        List<BlogPost> items = db.BlogPosts.Where(s => s.Id == 1).ToList();
        Assert.Single(items);

        LogEntry log = Assert.Single(context.Logs, log => log.EventId.Id == CommandExecuted);
        Assert.Contains(nameof(TestSimpleQuery), log.Message);
    }

    [Fact]
    public void TestTaggingVerbosity_FullStack()
    {
        string Test(CallSiteTaggingMode mode)
        {
            using TestDbContext context = new TestDbContext(x => x.TaggingMode = mode);

            using IServiceScope scope = context.CreateScope();
            Context db = scope.ServiceProvider.GetRequiredService<Context>();

            List<BlogPost> items = db.BlogPosts.Where(s => s.Id == 1).ToList();
            Assert.Single(items);

            LogEntry log = Assert.Single(context.Logs, log => log.EventId.Id == CommandExecuted);
            Assert.Contains(nameof(TestTaggingVerbosity_FullStack), log.Message);

            return log.Message;
        }

        // FullStack must include xunit & EF
        var logMessage = Test(CallSiteTaggingMode.FullStack);
        Assert.Contains("xunit", logMessage);
        Assert.Contains("Microsoft.EntityFrameworkCore.", logMessage);

        // FullStackToQuery must include xunit, but not EF
        // Xunit because we're executed by xunit
        logMessage = Test(CallSiteTaggingMode.FullStackToQuery);
        Assert.Contains("xunit", logMessage);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore.", logMessage);

        // OnlyMethod must not include xunit or EF
        // This should only contain the name of this method
        logMessage = Test(CallSiteTaggingMode.OnlyMethod);
        Assert.DoesNotContain("xunit", logMessage);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore.", logMessage);
    }
}