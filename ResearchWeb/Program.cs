using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ResearchWeb;
using ResearchWeb.Data;

// =====================================
// Render - File Watcher
// =====================================

Environment.SetEnvironmentVariable(
    "DOTNET_USE_POLLING_FILE_WATCHER",
    "1"
);

// =====================================
// Render Port
// =====================================

var port =
    Environment.GetEnvironmentVariable("PORT")
    ?? "5000";

Environment.SetEnvironmentVariable(
    "ASPNETCORE_URLS",
    $"http://0.0.0.0:{port}"
);

// =====================================
// Create Builder
// =====================================

var builder =
    WebApplication.CreateBuilder(
        new WebApplicationOptions
        {
            Args = args,
            EnvironmentName = Environments.Production
        }
    );

// =====================================
// Configuration
// =====================================

builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: false
    )
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: false
    )
    .AddEnvironmentVariables();

// =====================================
// Database
// =====================================

// Â–« «·„ €Ì— „ÊÃÊœ ›Ì Render ›ﬁÿ ⁄«œ…
var databaseUrl =
    Environment.GetEnvironmentVariable("DATABASE_URL");

// =====================================
// SQLite Path
// =====================================

var sqlitePath =
    Path.Combine(
        Directory.GetCurrentDirectory(),
        "App_Data",
        "research.db"
    );

Directory.CreateDirectory(
    Path.GetDirectoryName(sqlitePath)!
);

// =====================================
// Connection String
// =====================================

string connectionString;

// =====================================
// Render PostgreSQL
// =====================================

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    Console.WriteLine(
        "================================="
    );

    Console.WriteLine(
        "DATABASE = PostgreSQL"
    );

    Console.WriteLine(
        "DATABASE_URL detected."
    );

    Console.WriteLine(
        "================================="
    );

    var uri = new Uri(databaseUrl);

    var userInfo =
        uri.UserInfo.Split(':', 2);

    if (userInfo.Length == 0 ||
        string.IsNullOrWhiteSpace(userInfo[0]))
    {
        throw new InvalidOperationException(
            "DATABASE_URL username is missing."
        );
    }

    var builderConnection =
        new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,

            Port =
                uri.Port > 0
                    ? uri.Port
                    : 5432,

            Database =
                uri.AbsolutePath.TrimStart('/'),

            Username =
                Uri.UnescapeDataString(
                    userInfo[0]
                ),

            Password =
                userInfo.Length > 1
                    ? Uri.UnescapeDataString(
                        userInfo[1]
                    )
                    : "",

            SslMode = SslMode.Require
        };

    connectionString =
        builderConnection.ConnectionString;
}

// =====================================
// Local SQLite
// =====================================

else
{
    Console.WriteLine(
        "================================="
    );

    Console.WriteLine(
        "DATABASE = SQLite"
    );

    Console.WriteLine(
        "DATABASE PATH = " + sqlitePath
    );

    Console.WriteLine(
        "DATABASE EXISTS = " +
        File.Exists(sqlitePath)
    );

    Console.WriteLine(
        "================================="
    );

    connectionString =
        $"Data Source={sqlitePath}";
}

// =====================================
// Entity Framework
// =====================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            // =================================
            // Render PostgreSQL
            // =================================

            options.UseNpgsql(
                connectionString
            );
        }
        else
        {
            // =================================
            // Local SQLite
            // =================================

            options.UseSqlite(
                connectionString
            );
        }
    }
);

// =====================================
// MVC + API
// =====================================

builder.Services.AddControllersWithViews();

// =====================================
// Session
// =====================================

builder.Services.AddSession();

// =====================================
// Data Protection
// =====================================

var keysFolder =
    Path.Combine(
        Directory.GetCurrentDirectory(),
        "App_Data",
        "DataProtectionKeys"
    );

Directory.CreateDirectory(
    keysFolder
);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo(keysFolder)
    );

// =====================================
// Build
// =====================================

var app = builder.Build();

// =====================================
// Database Migration
// =====================================

using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    Console.WriteLine(
        "================================="
    );

    try
    {
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            // =================================
            // Render PostgreSQL
            // =================================

            Console.WriteLine(
                "DATABASE = PostgreSQL"
            );

            Console.WriteLine(
                "Applying PostgreSQL database migrations..."
            );

            db.Database.Migrate();

            Console.WriteLine(
                "PostgreSQL migrations completed."
            );
        }
        else
        {
            // =================================
            // Local SQLite
            // =================================

            Console.WriteLine(
                "DATABASE = SQLite"
            );

            Console.WriteLine(
                "Skipping SQLite migrations."
            );

            Console.WriteLine(
                "Using existing local SQLite database."
            );
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "DATABASE MIGRATION ERROR:"
        );

        Console.WriteLine(
            ex.ToString()
        );

        throw;
    }

    Console.WriteLine(
        "================================="
    );
}

// =====================================
// SQLite ? PostgreSQL Migration
// TEMPORARY
// =====================================

// Â–« «·„ €Ì— ·‰ ÌﬂÊ‰ „›⁄·« »‘ﬂ· ÿ»Ì⁄Ì.
// ”‰›⁄·Â ›ﬁÿ ⁄‰œ„« ‰—Ìœ ‰ﬁ· «·»Ì«‰« .

var migrateData =
    Environment.GetEnvironmentVariable(
        "MIGRATE_SQLITE_TO_POSTGRES"
    );

if (
    string.Equals(
        migrateData,
        "true",
        StringComparison.OrdinalIgnoreCase
    )
)
{
    if (string.IsNullOrWhiteSpace(databaseUrl))
    {
        Console.WriteLine(
            "================================="
        );

        Console.WriteLine(
            "ERROR: DATABASE_URL is not configured."
        );

        Console.WriteLine(
            "SQLite ? PostgreSQL migration cancelled."
        );

        Console.WriteLine(
            "================================="
        );
    }
    else
    {
        Console.WriteLine(
            "================================="
        );

        Console.WriteLine(
            "STARTING SQLITE ? POSTGRESQL TRANSFER"
        );

        Console.WriteLine(
            "================================="
        );

        try
        {
            await DataMigration.RunAsync(
                connectionString
            );

            Console.WriteLine(
                "================================="
            );

            Console.WriteLine(
                "SQLITE ? POSTGRESQL TRANSFER FINISHED"
            );

            Console.WriteLine(
                "================================="
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "================================="
            );

            Console.WriteLine(
                "DATA TRANSFER ERROR:"
            );

            Console.WriteLine(
                ex.ToString()
            );

            Console.WriteLine(
                "================================="
            );

            throw;
        }
    }
}

// =====================================
// Database Check
// =====================================

using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    Console.WriteLine(
        "================================="
    );

    try
    {
        Console.WriteLine(
            $"Research Count = {db.Researches.Count()}"
        );

        Console.WriteLine(
            $"Users count = {db.Users.Count()}"
        );

        Console.WriteLine(
            $"Visitors count = {db.Visitors.Count()}"
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "DATABASE CHECK ERROR:"
        );

        Console.WriteLine(
            ex.ToString()
        );

        throw;
    }

    Console.WriteLine(
        "================================="
    );
}

// =====================================
// Error Handling
// =====================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error"
    );
}

// =====================================
// Render Headers
// =====================================

app.UseForwardedHeaders(
    new ForwardedHeadersOptions
    {
        ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto
    }
);

// =====================================
// Static Files
// =====================================

app.UseStaticFiles();

// =====================================
// Routing
// =====================================

app.UseRouting();

// =====================================
// Session
// =====================================

app.UseSession();

// =====================================
// Authorization
// =====================================

app.UseAuthorization();

// =====================================
// API Controllers
// =====================================

app.MapControllers();

// =====================================
// MVC Default Route
// =====================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Login}/{action=Index}/{id?}"
);

// =====================================
// Run
// =====================================

app.Run();