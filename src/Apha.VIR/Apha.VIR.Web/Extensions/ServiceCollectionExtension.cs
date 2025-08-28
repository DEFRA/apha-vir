using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Repositories;

namespace Apha.VIR.Web.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddServices();
            services.AddRepositories();
            return services;
        }
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Add your application services here            
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IVirusCharacteristicService, VirusCharacteristicService>();
            services.AddScoped<IIsolateDispatchService, IsolateDispatchService>();
            services.AddScoped<IIsolateSearchService, IsolateSearchService>();
            services.AddScoped<IIsolateViabilityService, IsolateViabilityService>();
            services.AddScoped<IIsolatesService, IsolatesService>();
            services.AddScoped<ISystemInfoService, SystemInfoService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add your data access services here            
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<IVirusCharacteristicRepository, VirusCharacteristicRepository>();
            services.AddScoped<IIsolateDispatchRepository, DispatchRepository>();
            services.AddScoped<ICharacteristicRepository, CharacteristicRepository>();
            services.AddScoped<IIsolateRepository, IsolateRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();
            services.AddScoped<IWorkgroupRepository, WorkgroupRepository>();
            services.AddScoped<IVirusCharacteristicListEntryRepository, VirusCharacteristicListEntryRepository>();
            services.AddScoped<IIsolateSearchRepository, IsolateSearchRepository>();
            services.AddScoped<IIsolateViabilityRepository, IsolateViabilityRepository>();
            services.AddScoped<ISystemInfoRepository, SystemInfoRepository>();
            return services;
        }
    }
}
