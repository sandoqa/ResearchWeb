using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ResearchWeb.Data;
var builder = WebApplication.CreateBuilder(args);


// ÊÍÏíÏ ãßÇä ŞÇÚÏÉ ÇáÈíÇäÇÊ
AppDomain.CurrentDomain.SetData(
    "DataDirectory",
    Path.Combine(
        Directory.GetCurrentDirectory(),
        "App_Data"
    )
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
// ÅÖÇİÉ MVC
builder.Services.AddControllersWithViews();


// ÊİÚíá Session
builder.Services.AddSession();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// app.UseHttpsRedirection(); 
// íãßä ÅÈŞÇÄåÇ ÅĞÇ áÏíß HTTPS
// Ãæ ÊÚØíáåÇ ãÄŞÊğÇ ÃËäÇÁ ÇáÊÌÑÈÉ Úáì http

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