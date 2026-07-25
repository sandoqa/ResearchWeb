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

//  ‘€Ì· «· ÿ»Ìﬁ ⁄·Ï Render
builder.WebHost.UseUrls("http://0.0.0.0:5000");

builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: false
    );

//  ÕœÌœ „ﬂ«‰ ﬁ«⁄œ… «·»Ì«‰«  SQLite
var dbPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "App_Data",
    "research.db"
);

// «· √ﬂœ „‰ ÊÃÊœ «·„Ã·œ
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


var app = builder.Build();


// «Œ »«— ﬁ«⁄œ… «·»Ì«‰«  Ê≈ŸÂ«— ⁄œœ «·„” Œœ„Ì‰ ›Ì Render Logs
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


// „⁄«·Ã… «·√Œÿ«¡ ›Ì Production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// ≈⁄œ«œ«  Render
app.UseForwardedHeaders();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();


// «·„”«— «·«› —«÷Ì
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);


app.Run();