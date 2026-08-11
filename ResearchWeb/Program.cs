using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResearchWeb.Data;


// =====================================
// Render Fix - Disable File Watcher
// =====================================
Environment.SetEnvironmentVariable(
    "DOTNET_USE_POLLING_FILE_WATCHER",
    "true"
);


// =====================================
// Render Port
// =====================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

Environment.SetEnvironmentVariable(
    "ASPNETCORE_URLS",
    $"http://0.0.0.0:{port}"
);


// =====================================
// Create Builder
// =====================================
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Production
});


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
    );


// =====================================
// SQLite Database Path
// =====================================
var dbPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "App_Data",
    "research.db"
);
Console.WriteLine("DATABASE PATH = " + dbPath);
Console.WriteLine("DATABASE EXISTS = " + File.Exists(dbPath));
Console.WriteLine("RESEARCH COUNT = " + new ApplicationDbContext(
    new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlite($"Data Source={dbPath}")
        .Options
).Researches.Count());

Directory.CreateDirectory(
    Path.GetDirectoryName(dbPath)!
);


// =====================================
// Entity Framework SQLite
// =====================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
});


// =====================================
// MVC
// =====================================
builder.Services.AddControllersWithViews();


// =====================================
// Session
// =====================================
builder.Services.AddSession();


// =====================================
// Data Protection Keys
// Persistent Session Cookies
// =====================================
var keysFolder = Path.Combine(
    Directory.GetCurrentDirectory(),
    "App_Data",
    "DataProtectionKeys"
);


Directory.CreateDirectory(keysFolder);


builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo(keysFolder)
    );


// =====================================
// Build App
// =====================================
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    db.Database.Migrate();
}
// =====================================
// Database Check
// =====================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    db.Database.EnsureCreated();

    Console.WriteLine("=================================");
    Console.WriteLine($"Database Path = {dbPath}");
    Console.WriteLine($"Database Exists = {File.Exists(dbPath)}");
    Console.WriteLine($"Research Count = {db.Researches.Count()}");
    Console.WriteLine($"Users count = {db.Users.Count()}");
    Console.WriteLine("=================================");
}

// =====================================
// Error Handling
// =====================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}


// =====================================
// Render Headers
// =====================================
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});


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
// Default Route
// =====================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);


// =====================================
// Run
// =====================================
app.Run();