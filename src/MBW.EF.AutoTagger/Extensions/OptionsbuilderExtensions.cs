using System;
using MBW.EF.AutoTagger.Database;
using Microsoft.EntityFrameworkCore;

namespace MBW.EF.AutoTagger.Extensions;

public static class OptionsbuilderExtensions
{
    public static DbContextOptionsBuilder AddCallsiteTagging(this DbContextOptionsBuilder builder, Action<TaggedQueryCommandInterceptor> configure = null)
    {
        builder.AddInterceptors(new TaggedQueryCommandInterceptor());

        return builder;
    }

    public static DbContextOptionsBuilder<TContext> AddCallsiteTagging<TContext>(this DbContextOptionsBuilder<TContext> builder, Action<TaggedQueryCommandInterceptor> configure = null) where TContext : DbContext
    {
        builder.AddInterceptors(new TaggedQueryCommandInterceptor());

        return builder;
    }
}