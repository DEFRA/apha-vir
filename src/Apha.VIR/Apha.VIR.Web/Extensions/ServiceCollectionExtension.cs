using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.Web.Services;

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
            services.AddScoped<IReportService, ReportService>();
            services.AddSingleton<NavigationService, NavigationService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<ISenderService, SenderService>();
            services.AddScoped<ISubmissionService, SubmissionService>();
            services.AddScoped<ISampleService, SampleService>();
            services.AddScoped<ISystemInfoService, SystemInfoService>();
            services.AddScoped<IVirusCharacteristicListEntryService, VirusCharacteristicListEntryService>();
            services.AddScoped<IVirusCharacteristicAssociationService, VirusCharacteristicAssociationService>();
            services.AddScoped<IIsolateRelocateService, IsolateRelocateService>();
            services.AddSingleton<ICacheService, CacheService>();
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
            services.AddScoped<IVirusCharacteristicListEntryRepository, VirusCharacteristicListEntryRepository>();
            services.AddScoped<IIsolateSearchRepository, IsolateSearchRepository>();
            services.AddScoped<IIsolateViabilityRepository, IsolateViabilityRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<ISenderRepository, SenderRepository>();
            services.AddScoped<ISubmissionRepository, SubmissionRepository>();
            services.AddScoped<ISampleRepository, SampleRepository>();
            services.AddScoped<ISystemInfoRepository, SystemInfoRepository>();
            services.AddScoped<IVirusCharacteristicListEntryRepository, VirusCharacteristicListEntryRepository>();
            services.AddScoped<IVirusCharacteristicAssociationRepository, VirusCharacteristicAssociationRepository>();
            services.AddScoped<IIsolateRelocateRepository, IsolateRelocateRepository>();
            return services;
        }
    }
}
