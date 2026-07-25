using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ResearchWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// ÊÍÏíÏ ãßÇä ŞÇÚÏÉ ÇáÈíÇäÇÊ SQLite
var dbFolder = Path.Combine(
    Directory.GetCurrentDirectory(),
    "App_Data"
);

// ÅäÔÇÁ ÇáãÌáÏ ÅĞÇ áã íßä ãæÌæÏğÇ
Directory.CreateDirectory(dbFolder);

var dbPath = Path.Combine(
    dbFolder,
    "research.db"
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
);

// ÅÖÇİÉ MVC
builder.Services.AddControllersWithViews();

// ÊİÚíá Session
builder.Services.AddSession();

var app = builder.Build();

// ÅäÔÇÁ ŞÇÚÏÉ ÇáÈíÇäÇÊ æÇáÌÏÇæá ÅĞÇ áã Êßä ãæÌæÏÉ
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    db.Database.EnsureCreated();

    // ØÈÇÚÉ ÚÏÏ ÇáãÓÊÎÏãíä İí ÓÌá Render
    Console.WriteLine("=================================");
    Console.WriteLine($"Users count = {db.Users.Count()}");
    Console.WriteLine("=================================");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ÊÔÛíá ÇáãæŞÚ Îáİ Render
app.UseForwardedHeaders();

app.UseStaticFiles();

app.UseRouting();

// ÊÔÛíá Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"
);

app.Run();