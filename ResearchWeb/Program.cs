using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ResearchWeb.Data;

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

var databaseUrl =
    Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    // =================================
    // Render PostgreSQL
    // =================================

    Console.WriteLine(
        "DATABASE = PostgreSQL"
    );

    var uri = new Uri(databaseUrl);

    var userInfo =
        uri.UserInfo.Split(':', 2);

    var builderConnection =
        new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1
                ? Uri.UnescapeDataString(userInfo[1])
                : ""
        };

    connectionString =
        builderConnection.ConnectionString;
}
else
{
    // =================================
    // Local SQLite
    // =================================

    var dbPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "App_Data",
        "research.db"
    );

    Directory.CreateDirectory(
        Path.GetDirectoryName(dbPath)!
    );

    Console.WriteLine(
        "DATABASE = SQLite"
    );

    Console.WriteLine(
        "DATABASE PATH = " + dbPath
    );

    Console.WriteLine(
        "DATABASE EXISTS = " +
        File.Exists(dbPath)
    );

    connectionString =
        $"Data Source={dbPath}";
}

// =====================================
// Entity Framework
// =====================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            options.UseNpgsql(connectionString);
        }
        else
        {
            options.UseSqlite(connectionString);
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

var keysFolder = Path.Combine(
    Directory.GetCurrentDirectory(),
    "App_Data",
    "DataProtectionKeys"
);

Directory.CreateDirectory(keysFolder);

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

    Console.WriteLine(
        "Applying database migrations..."
    );

    db.Database.Migrate();

    Console.WriteLine(
        "Database migrations completed."
    );

    Console.WriteLine(
        "================================="
    );
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

    Console.WriteLine(
        $"Research Count = {db.Researches.Count()}"
    );

    Console.WriteLine(
        $"Users count = {db.Users.Count()}"
    );

    Console.WriteLine(
        $"Visitors count = {db.Visitors.Count()}"
    );

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
    pattern: "{controller=Login}/{action=Index}/{id?}"
);

// =====================================
// Run
// =====================================

app.Run();