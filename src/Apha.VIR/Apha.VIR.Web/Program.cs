using System.Globalization;
using Apha.VIR.Application.Mappings;
using Apha.VIR.Application.Validation;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.Web.Extensions;
using Apha.VIR.Web.Mappings;
using Apha.VIR.Web.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("local"))
{
    builder.Host.UseSerilog((ctx, lc) =>
    {
        lc.WriteTo.Console();
        string srvpath = ctx.Configuration.GetValue<string>("AppSettings:LosgPath") ?? string.Empty;
        string logpath = $"{(ctx.HostingEnvironment.IsDevelopment() ? "Logs" : srvpath)}\\Logsample.log";
        lc.WriteTo.File(logpath, Serilog.Events.LogEventLevel.Verbose, rollingInterval: RollingInterval.Day);
    });
}
else
{
    Serilog.Debugging.SelfLog.Enable(Console.Error);
    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration.UseAwsCloudWatch(builder.Configuration);
    });
}

// Add database context
builder.Services.AddDbContext<VIRDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VIRConnectionString")
    ?? throw new InvalidOperationException("Connection string 'VIRConnectionString' not found.")));

builder.Services.AddAutoMapper(typeof(EntityMapper).Assembly);
builder.Services.AddAutoMapper(typeof(ViewModelMapper).Assembly);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Services and Repositories
builder.Services.AddApplicationServices();

// Register Authentication services
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Set the default culture to en-GB (Great Britain)
var cultureSet = "en-GB";
var supportedCultures = new[] { new CultureInfo(cultureSet) };

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureSet),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
app.UseRequestLocalization(localizationOptions);
// Health checks endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("local"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();