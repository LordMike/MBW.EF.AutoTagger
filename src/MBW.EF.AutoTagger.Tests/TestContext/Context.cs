using Microsoft.EntityFrameworkCore;

namespace MBW.EF.AutoTagger.Tests.TestContext;

internal class Context : DbContext
{
    public DbSet<BlogPost> BlogPosts { get; set; }

    /// <summary>
    /// Allows the context to be instantiated using `.AddDbContext&lt;Context&gt;(.. options ..)`
    /// </summary>
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>()
            .HasData(new BlogPost
            {
                Id = 1,
                Title = "Test 1"
            }, new BlogPost
            {
                Id = 2,
                Title = "Test 2"
            });
    }
}