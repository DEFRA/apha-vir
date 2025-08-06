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
            CreateMap<SearchCriteria, SearchCriteriaDTO>();
            CreateMap<CharacteristicCriteria, CharacteristicCriteriaDTO>();
            CreateMap<IsolateSearchResultDTO, IsolateSearchResult>();
            CreateMap<IsolateSearchExportDto, IsolateSearchExportViewModel>();
        }
    }
}
