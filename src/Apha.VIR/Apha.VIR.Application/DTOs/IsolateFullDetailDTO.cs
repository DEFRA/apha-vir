using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.DTOs
{
    public class IsolateFullDetailDTO
    {
        public IsolateInfoDTO IsolateDetails { get; set; }
        public List<IsolateViabilityInfoDTO> IsolateViabilityDetails { get; set; }
        public List<IsolateDispatchInfoDTO> IsolateDispatchDetails { get; set; }
        public List<IsolateCharacteristicInfoDTO> IsolateCharacteristicDetails { get; set; }
    }
}
