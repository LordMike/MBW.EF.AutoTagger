# MBW.EF.AutoTagger [![Generic Build](https://github.com/LordMike/MBW.EF.AutoTagger/actions/workflows/dotnet.yml/badge.svg)](https://github.com/LordMike/MBW.EF.AutoTagger/actions/workflows/dotnet.yml) [![Nuget](https://img.shields.io/nuget/v/MBW.EF.AutoTagger)](https://www.nuget.org/packages/MBW.EF.AutoTagger)

-- todo --

Note: This project is not intended to be used in any other environment that testing or local development.

# Features

* Ability to automatically tag all queries made by an EF dbcontext
  * Functionality to tag with the calling site location of a query 

# Nuget packages

| Name | Nuget                                                                                                                    | Note |
|---|--------------------------------------------------------------------------------------------------------------------------|---|
| MBW.EF.AutoTagger | [![Nuget](https://img.shields.io/nuget/v/MBW.EF.AutoTagger)](https://www.nuget.org/packages/MBW.EF.AutoTagger/) | Core functionality |

# Usage example

-- todo --

This example is for a common use case: Using EntityFramework in an Asp.Net website.

Reference a package from above, depending on your target database, and then add it to your in-memory database context builder:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }
    public IHostEnvironment Environment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();

        // ... add other services

        // Add our DatabaseContext to our DI system - this switches implementation based on our hosting environment
        services.AddDbContext<DatabaseContext>(builder =>
        {
            // Only add validation when using the inmemory database. This ensures we don't have any adverse effect when running production environments
            if (Environment.IsDevelopment())
            {
                // In development, we want to use the in memory database with query validation
                // This ensures that in staging and production, we will know that any query made in development will successfully translate to SQL
                builder.UseInMemoryDatabase("LocalDatabase")
                    .AddMysqlExpressionValidation<DatabaseContext>();
            }
            else
            {
                // When in any other environment, we want to use an actual MySql database
                // Here, we will _not_ add validation
                builder.UseMySql("Server=localhost", new MySqlServerVersion(new Version(8, 0, 25)));
            }
        });
    }
}
```

Now - we can use our context like normal, but we'll know that all queries can translate to SQL.

```csharp
public class HomeController : Controller
{
    private readonly DatabaseContext _context;

    public HomeController(DatabaseContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // A query like this can easily translate to SQL and will always work
        _context.BlogPosts.Where(s => s.Id == 4).ToList();

        // A query like this could work in the in memory database, but not in actual databases
        // The expression validation will catch this, and throw an exception
        // This ensures that during local testing, we will not end up with a query that won't work in Mysql
        _context.BlogPosts.Where(s => s.Title.ToCharArray().Length == 2).ToList();

        return View();
    }
}
```
