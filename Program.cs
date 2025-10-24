using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB_SHOW_WRIST_STRAP.Core.Repositories;
using WEB_SHOW_WRIST_STRAP.Repositories;
using WEB_SHOW_WRIST_STRAP.Data;
using WEB_SHOW_WRIST_STRAP.Core;
using WEB_SHOW_WRIST_STRAP.Models.Entities;
using WEB_SHOW_WRIST_STRAP.Services;
using WEB_SHOW_WRIST_STRAP.Hubs;
using WEB_SHOW_WRIST_STRAP.Configs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});
var connectionString = WEB_SHOW_WRIST_STRAP.Dataconfig.ConnectStirng_User = builder.Configuration.GetConnectionString("UserLoginContextConnection") ?? throw new InvalidOperationException("Connection string 'UserLoginContextConnection' not found.");
var connectionString2 = WEB_SHOW_WRIST_STRAP.Dataconfig.ConnectString_Data = builder.Configuration.GetConnectionString("PointDataStringConnection") ?? throw new InvalidOperationException("Connection string 'LEDDataStringConnection' not found.");
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment(); // Hi?n th? l?i chi ti?t khi debug
    options.KeepAliveInterval = TimeSpan.FromSeconds(15); // G?i tín hi?u keep-alive
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30); // Timeout n?u client không ph?n h?i
});
builder.Services.AddHttpClient();
builder.Services.AddDbContext<UserLoginContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<DataPointContext>(options =>
    options.UseSqlServer(connectionString2));
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<UserLoginContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<MonitoringBackgroundService>();
builder.Services.AddHostedService<MonitoringBackgroundService2>();

// Đăng ký Dapper factory với connection string 2 (PointDataStringConnection)
builder.Services.AddScoped<IDbConnectionFactory>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("PointDataStringConnection")
        ?? throw new InvalidOperationException("Connection string 'PointDataStringConnection' not found.");
    return new DbConnectionFactory(connectionString);
});
#region Authorization

AddAuthorizationPolicies();

#endregion

AddScoped();

var app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<MonitoringHub>("/monitoringHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();


void AddAuthorizationPolicies()
{
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("EmployeeNumber"));
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(Constants.Policies.RequireAdmin, policy => policy.RequireRole(Constants.Roles.Administrator));
        options.AddPolicy(Constants.Policies.RequireManager, policy => policy.RequireRole(Constants.Roles.Manager));
    });
}

void AddScoped()
{
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRoleRepository, RoleRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
}
