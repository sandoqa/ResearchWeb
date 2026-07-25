using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ResearchWeb.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Production
});


//  ‘€Ì· «· ÿ»Ìﬁ ⁄·Ï Render »«” Œœ«„ PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


// ≈⁄œ«œ«  JSON
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


// „”«— ﬁ«⁄œ… «·»Ì«‰«  SQLite
var dbPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "App_Data",
    "research.db"
);


// ≈‰‘«¡ „Ã·œ ﬁ«⁄œ… «·»Ì«‰« 
Directory.CreateDirectory(
    Path.GetDirectoryName(dbPath)!
);


// —»ÿ SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
);


// MVC
builder.Services.AddControllersWithViews();


// Session
builder.Services.AddSession();


// ≈‰‘«¡ «· ÿ»Ìﬁ
var app = builder.Build();


// ≈‰‘«¡ ﬁ«⁄œ… «·»Ì«‰«  Ê›Õ’ «·„” Œœ„Ì‰
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    db.Database.EnsureCreated();

    Console.WriteLine("=================================");
    Console.WriteLine($"Database Path = {dbPath}");
    Console.WriteLine($"Users count = {db.Users.Count()}");
    Console.WriteLine("=================================");
}



// ≈⁄œ«œ«  Production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}



// „Â„ ·‹ Render
app.UseForwardedHeaders();


// «·„·›«  «·À«» …
app.UseStaticFiles();


// Routing
app.UseRouting();


// Session
app.UseSession();


// Authorization
app.UseAuthorization();


// «·’›Õ… «·«› —«÷Ì…
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);


//  ‘€Ì· «· ÿ»Ìﬁ
app.Run();