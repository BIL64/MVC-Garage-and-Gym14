using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Garage3BL.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<Garage3BLContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Garage3BLContext") ?? throw new InvalidOperationException("Connection string 'Garage3BLContext' not found.")));

string text = "First line" + Environment.NewLine;

// The Tempdata provider cookie is not essential. Make it essential
// so Tempdata is functional when tracking is disabled.
//builder.Services.Configure<CookieTempDataProviderOptions>(options => {
//    options.Cookie.IsEssential = true;
//});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Tillägg av BL
//builder.Services.AddHttpContextAccessor();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
