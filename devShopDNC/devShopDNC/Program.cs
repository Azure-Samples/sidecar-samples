using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using devShopDNC;

var builder = WebApplication.CreateBuilder(args);
var startup=new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

//builder.Services.AddDbContext<devShopDNCContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("devShopDNCContext") ?? throw new InvalidOperationException("Connection string 'devShopDNCContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Building the environment
var app = builder.Build();
startup.Configure(app,builder.Environment);

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
