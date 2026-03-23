using System.Text;
using Microsoft.EntityFrameworkCore;
using LoginNet.Data;
using LoginNet.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=LoginNetDB;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<EmailService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".LoginNet.Session";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
app.Services.GetRequiredService<ILogger<Program>>()
    .LogInformation("TimeZone configured: {TimeZone}", timeZone.DisplayName);

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Account/Login");
}

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }
