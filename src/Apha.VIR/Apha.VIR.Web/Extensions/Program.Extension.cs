using Apha.VIR.Application.Mappings;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.Web.Mappings;
using Apha.VIR.Web.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Apha.VIR.Web.Extensions
{
    public static class ProgramExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            // Add database context
            services.AddDbContext<VIRDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("VIRConnectionString")
                ?? throw new InvalidOperationException("Database Connection string 'VIRConnectionString' not found.")));

            // AutoMapper
            services.AddAutoMapper(typeof(EntityMapper).Assembly);
            services.AddAutoMapper(typeof(ViewModelMapper));

            // MVC
            services.AddControllersWithViews();

            // Application services
            services.AddApplicationServices();

            // Authentication
            services.AddAuthenticationServices(configuration);

            // HTTP Context
            services.AddHttpContextAccessor();

            // Health checks
            services.AddHealthChecks();
        }

        public static void ConfigureMiddleware(this WebApplication app)
        {
            var env = app.Environment;

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

            // Error handling
            if (env.IsDevelopment() || env.IsEnvironment("local"))
            {
                app.UseDeveloperExceptionPage();
            }
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();



            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}