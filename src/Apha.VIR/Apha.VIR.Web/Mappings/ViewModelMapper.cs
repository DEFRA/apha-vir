using Apha.VIR.Application.DTOs;
using Apha.VIR.Web.Models;
using AutoMapper;

namespace Apha.VIR.Web.Mappings
{
    public class ViewModelMapper : Profile
    {
        public ViewModelMapper()
        {
            CreateMap<LookupDTO, LookupViewModel>();
            CreateMap<IsolateDispatchHistory, IsolateDispatchInfoDTO>().ReverseMap();
            CreateMap<SearchCriteria, SearchRepositoryViewModel>();
            CreateMap<CharacteristicCriteria, CharacteristicSearchViewModel>();
            CreateMap<SearchCriteria, SearchCriteriaDTO>().ReverseMap();
            CreateMap<CharacteristicCriteria, CharacteristicCriteriaDTO>().ReverseMap();
            CreateMap<IsolateSearchResultDTO, IsolateSearchResult>();
            CreateMap<IsolateSearchExportDto, IsolateSearchExportViewModel>();
            CreateMap<IsolateViabilityInfoDTO, IsolateViabilityModel>();
            CreateMap<IsolateFullDetailDTO, IsolateDetailsViewModel>();
            CreateMap<IsolateInfoDTO, IsolateDetails>();
            CreateMap<IsolateViabilityInfoDTO, IsolateViabilityCheckInfo>();
            CreateMap<IsolateDispatchInfoDTO, IsolateDispatchInfo>();
            CreateMap<IsolateCharacteristicInfoDTO, IsolateCharacteristicInfo>();
            CreateMap<IsolateDispatchInfoDTO, IsolateDispatchEditViewModel>().ReverseMap();

        }
    }
}
