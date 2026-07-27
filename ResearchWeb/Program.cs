using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResearchWeb.Data;
using ResearchWeb.Models;


// √Œ– «·„‰›– „‰ Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

Environment.SetEnvironmentVariable(
    "ASPNETCORE_URLS",
    $"http://0.0.0.0:{port}"
);


// ≈‰‘«¡ «· ÿ»Ìﬁ
var builder = WebApplication.CreateBuilder(args);

builder.Environment.EnvironmentName = Environments.Production;


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


// »‰«¡ «· ÿ»Ìﬁ
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


// „⁄«·Ã… «·√Œÿ«¡
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}


// ≈⁄œ«œ«  Render
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


//  ‘€Ì· «·„Êﬁ⁄
app.Run();