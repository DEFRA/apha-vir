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
            CreateMap<Lookup, LookupDTO>().ReverseMap();
            CreateMap<LookupItem, LookupItemDTO>().ReverseMap();
            CreateMap<VirusCharacteristic, VirusCharacteristicDTO>().ReverseMap();
            CreateMap<VirusCharacteristicListEntry, VirusCharacteristicListEntryDTO>().ReverseMap();
            CreateMap<SearchCriteriaDTO, SearchCriteria>();
            CreateMap<CharacteristicCriteriaDTO, CharacteristicCriteria>();
            CreateMap<IsolateSearchResult, IsolateSearchResultDTO>();
            CreateMap<QueryParameters<SearchCriteriaDTO>, PaginationParameters<SearchCriteria>>();
            CreateMap<PagedData<IsolateSearchResult>, PaginatedResult<IsolateSearchResultDTO>>(); 
            CreateMap<IsolateInfoDTO, IsolateSearchExportDto>()
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
            CreateMap<IsolateFullDetail, IsolateFullDetailDTO>().ReverseMap();
            CreateMap<IsolateInfoDTO, IsolateInfo>().ReverseMap();
            CreateMap<IsolateDispatchInfoDTO, IsolateDispatchInfo>().ReverseMap();
            CreateMap<IsolateCharacteristicInfoDTO, IsolateCharacteristicInfo>().ReverseMap();
            CreateMap<IsolateViabilityInfoDTO, IsolateViabilityInfo>().ReverseMap();
            CreateMap<IsolateViabilityInfo, IsolateViability>().ReverseMap();
            CreateMap<IsolateViability, IsolateViabilityDTO>().ReverseMap();
            CreateMap<IsolateViabilityInfoDTO, IsolateViability>().ReverseMap();
        }
    }
}
