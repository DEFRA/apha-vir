using Apha.VIR.Application.DTOs;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.AuditLog;
using Apha.VIR.Web.Models.Lookup;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;

namespace Apha.VIR.Web.Mappings
{
    public class ViewModelMapper : Profile
    {
        public ViewModelMapper()
        {
            CreateMap<LookupDto, LookupViewModel>();
            CreateMap<LookupItemDto, LookupItemModel>().ReverseMap();
            CreateMap<IsolateDispatchHistory, IsolateDispatchInfoDto>().ReverseMap();
            CreateMap<SearchCriteria, SearchRepositoryViewModel>();
            CreateMap<CharacteristicCriteria, CharacteristicSearchViewModel>();
            CreateMap<SearchCriteria, SearchCriteriaDTO>().ReverseMap();
            CreateMap<CharacteristicCriteria, CharacteristicCriteriaDto>().ReverseMap();
            CreateMap<IsolateSearchResultDto, IsolateSearchResult>();
            CreateMap<IsolateSearchExportDto, IsolateSearchExportViewModel>();
            CreateMap<IsolateFullDetailDto, IsolateDetailsViewModel>();
            CreateMap<IsolateInfoDto, IsolateDetails>();
            CreateMap<IsolateViabilityInfoDto, IsolateViabilityCheckInfo>();
            CreateMap<IsolateDispatchInfoDto, IsolateDispatchInfo>();
            CreateMap<IsolateCharacteristicInfoDto, IsolateCharacteristicInfo>().ReverseMap();
            CreateMap<IsolateCharacteristicDto, IsolateCharacteristicViewModel>().ReverseMap();
            CreateMap<IsolateDispatchInfoDto, IsolateDispatchEditViewModel>().ReverseMap();
            CreateMap<IsolateViabilityInfoDto, IsolateViabilityModel>().ReverseMap();
            CreateMap<IsolateDispatchCreateViewModel, IsolateDispatchInfoDto>();
            CreateMap<IsolateDispatchReportDto, IsolateDispatchReportModel>().ReverseMap();
            CreateMap<AuditCharacteristicLogDto, AuditCharacteristicsLogModel>().ReverseMap();
            CreateMap<AuditDispatchLogDto, AuditDispatchLogModel>().ReverseMap();
            CreateMap<AuditIsolateLogDto, AuditIsolateLogModel>().ReverseMap();
            CreateMap<AuditSampleLogDto, AuditSampleLogModel>().ReverseMap();
            CreateMap<AuditSubmissionLogDto, AuditSubmissionLogModel>().ReverseMap();
            CreateMap<AuditViabilityLogDto, AuditIsolateViabilityLogModel>().ReverseMap();
            CreateMap<AuditIsolateLogDetailDto, AuditIsolateLogDetailsViewModel>().ReverseMap();
            CreateMap<SenderDto, SubmissionSenderViewModel>().ReverseMap();
            CreateMap<SubmissionDto, SubmissionEditViewModel>().ReverseMap();
            CreateMap<SubmissionDto, SubmissionCreateViewModel>().ReverseMap();
            CreateMap<SampleDto, SampleViewModel>().ReverseMap();
            CreateMap<SystemInfoDto, SystemInformationViewModel>().ReverseMap();
            CreateMap<SenderDto, SenderViewModel>().ReverseMap();
            CreateMap<IsolateAddEditViewModel, IsolateDto>().ReverseMap();
            CreateMap<IsolateAddEditViewModel, IsolateViabilityInfoDto>()
                .ForMember(dest => dest.IsolateViabilityIsolateId, opt => opt.MapFrom(src => src.IsolateId))
                .ForMember(dest => dest.CheckedById, opt => opt.MapFrom(src => src.CheckedBy));
            CreateMap<VirusCharacteristicListEntryDto, VirusCharacteristicListEntryModel>().ReverseMap();
            CreateMap<SampleDto, SubmissionSamplesModel>();
            CreateMap<IsolateInfoDto, SubmissionIsolatesModel>();
            CreateMap<IsolateRelocateDto, IsolateRelocateViewModel>().ReverseMap();
        }
    }
}
