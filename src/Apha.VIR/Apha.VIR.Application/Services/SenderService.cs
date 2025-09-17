using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public async Task<IEnumerable<SenderDto>> GetAllSenderOrderByOrganisationAsync(Guid? countryId)
        {
            var senders = await _senderRepository.GetAllSenderOrderByOrganisationAsync(countryId);

            return _mapper.Map<IEnumerable<SenderDto>>(senders);
        }

        public async Task<IEnumerable<SenderDto>> GetAllSenderOrderBySenderAsync(Guid? countryId)
        {
            var senders = await _senderRepository.GetAllSenderOrderBySenderAsync(countryId);

            return _mapper.Map<IEnumerable<SenderDto>>(senders);
        }

        public async Task<PaginatedResult<SenderDto>> GetAllSenderAsync(int pageNo, int pageSize)
        {
            return _mapper.Map<PaginatedResult<SenderDto>>(
                await _senderRepository.GetAllSenderAsync(pageNo, pageSize));
        }

        public async Task<SenderDto> GetSenderAsync(Guid senderId)
        {
            return _mapper.Map<SenderDto>(
                await _senderRepository.GetSenderAsync(senderId));
        }

        public async Task AddSenderAsync(SenderDto sender)
        {
            var senderData = _mapper.Map<Sender>(sender);

            await _senderRepository.AddSenderAsync(senderData);
        }

        public async Task UpdateSenderAsync(SenderDto sender)
        {
            var senderData = _mapper.Map<Sender>(sender);

            await _senderRepository.UpdateSenderAsync(senderData);
        }

        public async Task DeleteSenderAsync(Guid senderId)
        {
            await _senderRepository.DeleteSenderAsync(senderId);
        }
    }
}
