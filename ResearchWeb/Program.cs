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
builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

//  ÕœÌœ „ﬂ«‰ ﬁ«⁄œ… «·»Ì«‰«  SQLite
var dbFolder = Path.Combine(
    Directory.GetCurrentDirectory(),
    "App_Data"
);

// ≈‰‘«¡ «·„Ã·œ ≈–« ·„ Ìﬂ‰ „ÊÃÊœ«
Directory.CreateDirectory(dbFolder);

var dbPath = Path.Combine(
    dbFolder,
    "research.db"
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
);

// ≈÷«›… MVC
builder.Services.AddControllersWithViews();

//  ›⁄Ì· Session
builder.Services.AddSession();

var app = builder.Build();

// ≈‰‘«¡ ﬁ«⁄œ… «·»Ì«‰«  Ê«·Ãœ«Ê· ≈–« ·„  ﬂ‰ „ÊÃÊœ…
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    db.Database.EnsureCreated();

    // ÿ»«⁄… ⁄œœ «·„” Œœ„Ì‰ ›Ì ”Ã· Render
    Console.WriteLine("=================================");
    Console.WriteLine($"Users count = {db.Users.Count()}");
    Console.WriteLine("=================================");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//  ‘€Ì· «·„Êﬁ⁄ Œ·› Render
app.UseForwardedHeaders();

app.UseStaticFiles();

app.UseRouting();

//  ‘€Ì· Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);

app.Run();