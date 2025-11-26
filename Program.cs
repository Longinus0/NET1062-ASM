using Microsoft.EntityFrameworkCore;
using ASM.Data;   

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FastFoodDbContext>(options =>
    options.UseSqlite("Data Source=fastfood.db"));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
