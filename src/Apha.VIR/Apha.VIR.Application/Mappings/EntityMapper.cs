using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;
using AutoMapper;

namespace Apha.VIR.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap<Lookup, LookupDto>().ReverseMap();
            CreateMap<LookupItem, LookupItemDto>().ReverseMap();
            CreateMap<PagedData<LookupItem>, PaginatedResult<LookupItemDto>>();
            CreateMap<VirusCharacteristic, VirusCharacteristicDto>().ReverseMap();
            CreateMap<VirusCharacteristicListEntry, VirusCharacteristicListEntryDto>().ReverseMap();
            CreateMap<SearchCriteriaDTO, SearchCriteria>();
            CreateMap<CharacteristicCriteriaDto, CharacteristicCriteria>();
            CreateMap<IsolateSearchResult, IsolateSearchResultDto>();
            CreateMap<QueryParameters<SearchCriteriaDTO>, PaginationParameters<SearchCriteria>>();
            CreateMap<PagedData<IsolateSearchResult>, PaginatedResult<IsolateSearchResultDto>>();
            CreateMap<IsolateInfoDto, IsolateSearchExportDto>()
             .ForMember(dest => dest.VirusFamily, opt => opt.MapFrom(src => src.FamilyName))
             .ForMember(dest => dest.VirusType, opt => opt.MapFrom(src => src.TypeName))
             .ForMember(dest => dest.HostPurpose, opt => opt.MapFrom(src => src.HostPurposeName))
             .ForMember(dest => dest.SampleType, opt => opt.MapFrom(src => src.SampleTypeName))
             .ForMember(dest => dest.Group, opt => opt.MapFrom(src => src.GroupSpeciesName))
             .ForMember(dest => dest.Species, opt => opt.MapFrom(src => src.BreedName))
             .ForMember(dest => dest.CountryOfOrigin, opt => opt.MapFrom(src => src.CountryOfOriginName))
             .ForMember(dest => dest.IsolationMethod, opt => opt.MapFrom(src => src.IsolationMethodName))
             .ForMember(dest => dest.Freezer, opt => opt.MapFrom(src => src.FreezerName))
             .ForMember(dest => dest.Tray, opt => opt.MapFrom(src => src.TrayName))
             .ForMember(dest => dest.IsMixedIsolate, opt => opt.MapFrom(src => MappingHelper.ToYesNo(src.IsMixedIsolate)))
             .ForMember(dest => dest.ValidToIssue, opt => opt.MapFrom(src => MappingHelper.ToYesNo(src.ValidToIssue)))
             .ForMember(dest => dest.OriginalSampleAvailable, opt => opt.MapFrom(src => MappingHelper.ToYesNo(src.OriginalSampleAvailable)))
             .ForMember(dest => dest.AntiserumProduced, opt => opt.MapFrom(src => MappingHelper.ToYesNo(src.AntiserumProduced)))
             .ForMember(dest => dest.OriginalSampleAvailable, opt => opt.MapFrom(src => MappingHelper.ToYesNo(src.OriginalSampleAvailable)))
             .ForMember(dest => dest.AntigenProduced, opt => opt.MapFrom(src => MappingHelper.ToYesNo(src.AntigenProduced)))
             .ForMember(dest => dest.MTA, opt => opt.MapFrom(src => MappingHelper.ToYesNo(src.MaterialTransferAgreement)))
             .ForMember(dest => dest.ReceivedDate, opt => opt.MapFrom(src => MappingHelper.ToDateStringFormat(src.ReceivedDate)));
            CreateMap<IsolateFullDetail, IsolateFullDetailDto>().ReverseMap();
            CreateMap<IsolateInfoDto, IsolateInfo>().ReverseMap();
            CreateMap<IsolateDispatchInfoDto, IsolateDispatchInfo>()
             .ForMember(dest => dest.NoOfAliquots, opt => opt.MapFrom(src => src.NoOfAliquotsToBeDispatched))
             .ForMember(dest => dest.IsolateNoOfAliquots, opt => opt.MapFrom(src => src.NoOfAliquots)).ReverseMap();
            CreateMap<IsolateCharacteristicDto, IsolateCharacteristicInfo>().ReverseMap();
            CreateMap<IsolateCharacteristicInfoDto, IsolateCharacteristicInfo>().ReverseMap();
            CreateMap<IsolateViabilityInfoDto, IsolateViabilityInfo>().ReverseMap();
            CreateMap<IsolateViabilityInfo, IsolateViability>().ReverseMap();
            CreateMap<IsolateViability, IsolateViabilityDto>().ReverseMap();
            CreateMap<IsolateViabilityInfoDto, IsolateViability>().ReverseMap();
            CreateMap<IsolateDispatchReportDto, IsolateDispatchInfo>().ReverseMap();
            CreateMap<AuditCharacteristicLogDto, AuditCharacteristicLog>().ReverseMap();
            CreateMap<AuditDispatchLogDto, AuditDispatchLog>().ReverseMap();
            CreateMap<AuditIsolateLogDto, AuditIsolateLog>().ReverseMap();
            CreateMap<AuditSampleLogDto, AuditSampleLog>().ReverseMap();
            CreateMap<AuditSubmissionLogDto, AuditSubmissionLog>().ReverseMap();
            CreateMap<AuditViabilityLogDto, AuditViabilityLog>().ReverseMap();
            CreateMap<AuditIsolateLogDetailDto, AuditIsolateLogDetail>().ReverseMap();
            CreateMap<SenderDto, Sender>().ReverseMap();
            CreateMap<SubmissionDto, Submission>().ReverseMap();
            CreateMap<Sample, SampleDto>().ReverseMap();
            CreateMap<SampleDto, Sample>().ReverseMap();
            CreateMap<SystemInfo, SystemInfoDto>().ReverseMap();
            CreateMap<IsolateDto, Isolate>().ReverseMap();
            CreateMap<PagedData<Sender>, PaginatedResult<SenderDto>>();
            CreateMap<PagedData<VirusCharacteristicListEntry>, PaginatedResult<VirusCharacteristicListEntryDto>>();
            CreateMap<IsolateRelocate, IsolateRelocateDto>().ReverseMap();
        }
    }
}
