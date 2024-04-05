using System.Collections.Generic;
using System.Linq;
using MBW.EF.AutoTagger.Tests.TestContext;
using MBW.EF.AutoTagger.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MBW.EF.AutoTagger.Tests;

public class EdgeCaseTests
{
    private const int CommandExecuted = 20101;

    /// <summary>
    /// If no frame matches our filters, we should not fail but default to not showing anything
    /// </summary>
    [Fact]
    public void NoMatchingFrame()
    {
        using TestDbContext context = new TestDbContext(x => x.FrameFilter = (_, _) => false);

        using IServiceScope scope = context.CreateScope();
        Context db = scope.ServiceProvider.GetRequiredService<Context>();

        List<BlogPost> items = db.BlogPosts.Where(s => s.Id == 1).ToList();
        Assert.Single(items);

        LogEntry log = Assert.Single(context.Logs, log => log.EventId.Id == CommandExecuted);
        Assert.DoesNotContain(nameof(NoMatchingFrame), log.Message);
        Assert.DoesNotContain("--", log.Message);
    }
}