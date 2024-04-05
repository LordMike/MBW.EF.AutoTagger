using System;
using MBW.EF.AutoTagger.Database;
using Microsoft.EntityFrameworkCore;

namespace MBW.EF.AutoTagger.Extensions;

public static class OptionsbuilderExtensions
{
    public static DbContextOptionsBuilder UseCallsiteTagging(this DbContextOptionsBuilder builder, Action<CallSiteTaggerConfig>? configure = null)
    {
        CallSiteTaggerConfig config = new CallSiteTaggerConfig();
        configure?.Invoke(config);
        
        builder.AddInterceptors(new CallSiteTagger(config));

        return builder;
    }

    public static DbContextOptionsBuilder<TContext> UseCallsiteTagging<TContext>(this DbContextOptionsBuilder<TContext> builder, Action<CallSiteTaggerConfig>? configure = null) where TContext : DbContext
    {
        CallSiteTaggerConfig config = new CallSiteTaggerConfig();
        configure?.Invoke(config);

        builder.AddInterceptors(new CallSiteTagger(config));

        return builder;
    }
}