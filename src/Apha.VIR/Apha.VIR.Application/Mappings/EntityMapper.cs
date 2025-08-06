using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;
using AutoMapper;

namespace Apha.VIR.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap<Lookup, LookupDTO>().ReverseMap();
            CreateMap<LookupItem, LookupItemDTO>().ReverseMap();
            CreateMap<LookupItem, LookupItemDTO>().ReverseMap();
            CreateMap<VirusCharacteristic, VirusCharacteristicDTO>().ReverseMap();
            CreateMap<IsolateDispatchInfo, IsolateDispatchInfoDTO>().ReverseMap();
        }
    }
}
