using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class SenderService : ISenderService
    {
        private readonly ISenderRepository _senderRepository;
        private readonly IMapper _mapper;

        public SenderService(ISenderRepository senderRepository,
            IMapper mapper)
        {
            _senderRepository = senderRepository ?? throw new ArgumentNullException(nameof(senderRepository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<SenderDTO>> GetAllSenderOrderByOrganisationAsync(Guid? countryId)
        {
            var senders = await _senderRepository.GetAllSenderOrderByOrganisationAsync(countryId);
            return _mapper.Map<IEnumerable<SenderDTO>>(senders);
        }

        public async Task<IEnumerable<SenderDTO>> GetAllSenderOrderBySenderAsync(Guid? countryId)
        {
            var senders = await _senderRepository.GetAllSenderOrderBySenderAsync(countryId);
            return _mapper.Map<IEnumerable<SenderDTO>>(senders);
        }

        public async Task<PaginatedResult<SenderDTO>> GetAllSenderAsync(int pageNo, int pageSize)
        {
            return _mapper.Map<PaginatedResult<SenderDTO>>(
                await _senderRepository.GetAllSenderAsync(pageNo, pageSize));
        }

        public async Task AddSenderAsync(SenderDTO sender)
        {
            var senderData = _mapper.Map<Sender>(sender);
            await _senderRepository.AddSenderAsync(senderData);
        }
    }
}
