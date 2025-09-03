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
            CreateMap<LookupDTO, LookupViewModel>();
            CreateMap<LookupItemDTO, LookupItemModel>().ReverseMap();
            CreateMap<IsolateDispatchHistory, IsolateDispatchInfoDTO>().ReverseMap();
            CreateMap<SearchCriteria, SearchRepositoryViewModel>();
            CreateMap<CharacteristicCriteria, CharacteristicSearchViewModel>();
            CreateMap<SearchCriteria, SearchCriteriaDTO>().ReverseMap();
            CreateMap<CharacteristicCriteria, CharacteristicCriteriaDTO>().ReverseMap();
            CreateMap<IsolateSearchResultDTO, IsolateSearchResult>();
            CreateMap<IsolateSearchExportDto, IsolateSearchExportViewModel>();
            CreateMap<IsolateFullDetailDTO, IsolateDetailsViewModel>();
            CreateMap<IsolateInfoDTO, IsolateDetails>();
            CreateMap<IsolateViabilityInfoDTO, IsolateViabilityCheckInfo>();
            CreateMap<IsolateDispatchInfoDTO, IsolateDispatchInfo>();
            CreateMap<IsolateCharacteristicInfoDTO, IsolateCharacteristicInfoModel>().ReverseMap();
            CreateMap<IsolateDispatchInfoDTO, IsolateDispatchEditViewModel>().ReverseMap();
            CreateMap<IsolateViabilityInfoDTO, IsolateViabilityModel>().ReverseMap();
            CreateMap<IsolateDispatchCreateViewModel, IsolateDispatchInfoDTO>();
            CreateMap<IsolateDispatchReportDTO, IsolateDispatchReportModel>().ReverseMap();
            CreateMap<AuditCharacteristicLogDTO, AuditCharacteristicsLogModel>().ReverseMap();
            CreateMap<AuditDispatchLogDTO, AuditDispatchLogModel>().ReverseMap();
            CreateMap<AuditIsolateLogDTO, AuditIsolateLogModel>().ReverseMap();
            CreateMap<AuditSampleLogDTO, AuditSampleLogModel>().ReverseMap();
            CreateMap<AuditSubmissionLogDTO, AuditSubmissionLogModel>().ReverseMap();
            CreateMap<AuditViabilityLogDTO, AuditIsolateViabilityLogModel>().ReverseMap();
            CreateMap<AuditIsolateLogDetailDTO, AuditIsolateLogDetailsViewModel>().ReverseMap();
            CreateMap<SenderDTO, SubmissionSenderViewModel>().ReverseMap();
            CreateMap<SubmissionDTO, SubmissionEditViewModel>().ReverseMap();
            CreateMap<SubmissionDTO, SubmissionCreateViewModel>().ReverseMap();
            CreateMap<SampleDTO, SampleViewModel>().ReverseMap();
            CreateMap<SystemInfoDTO, SystemInformationViewModel>().ReverseMap();
            CreateMap<SenderDTO, SenderViewModel>().ReverseMap();
            CreateMap<IsolateAddEditViewModel, IsolateDTO>().ReverseMap();
            CreateMap<IsolateAddEditViewModel, IsolateViabilityInfoDTO>()
                .ForMember(dest => dest.IsolateViabilityIsolateId, opt => opt.MapFrom(src => src.IsolateId))
                .ForMember(dest => dest.CheckedById, opt => opt.MapFrom(src => src.CheckedBy));
            CreateMap<VirusCharacteristicListEntryDTO, VirusCharacteristicListEntryModel>().ReverseMap();
        }
    }
}
