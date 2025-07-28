using Apha.VIR.Application.DTOs;
using Apha.VIR.Web.Models;
using AutoMapper;

namespace Apha.VIR.Web.Mappings
{
    public class ViewModelMapper : Profile
    {
        public ViewModelMapper()
        {
            // CreateMap<SourceType, DestinationType>();
            // Add your DTO to viewmodel mappings here
            // For example:
            CreateMap<LookupDTO, LookupViewModel>();
        }
    }
}
