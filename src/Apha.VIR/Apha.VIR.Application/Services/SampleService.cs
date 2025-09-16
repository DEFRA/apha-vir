using System;
using System.Net.Http.Headers;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Components;

namespace Apha.VIR.Application.Services
{
    public class SampleService : ISampleService
    {
        private readonly ISampleRepository _sampleRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;

        public SampleService(ISampleRepository sampleRepository, ILookupRepository lookupRepository, IMapper mapper)
        {
            _sampleRepository = sampleRepository ?? throw new ArgumentNullException(nameof(sampleRepository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<SampleDto>> GetSamplesBySubmissionIdAsync(Guid submissionId)
        {
            var samples = await _sampleRepository.GetSamplesBySubmissionIdAsync(submissionId);            
            var samplesDto = _mapper.Map<IEnumerable<SampleDto>>(samples);
            var hostBreeds = await _lookupRepository.GetAllHostBreedsAsync();
            var hostSpecies = await _lookupRepository.GetAllHostSpeciesAsync();
            var sampleTypes = await _lookupRepository.GetAllSampleTypesAsync();
            var hostPurposes = await _lookupRepository.GetAllHostPurposesAsync();
            foreach (var sample in samplesDto)
            {
                sample.HostBreedName = hostBreeds?.FirstOrDefault(wg => wg.Id == sample.HostBreed!.Value)?.Name;
                sample.HostSpeciesName = hostSpecies?.FirstOrDefault(wg => wg.Id == sample.HostSpecies!.Value)?.Name;
                sample.SampleTypeName = sampleTypes?.FirstOrDefault(wg => wg.Id == sample.SampleType!.Value)?.Name;
                sample.HostPurposeName = hostPurposes?.FirstOrDefault(wg => wg.Id == sample.HostPurpose!.Value)?.Name;
            }
            return samplesDto;
        }

        public async Task<SampleDto> GetSampleAsync(string avNumber, Guid? sampleId)
        {
            ArgumentNullException.ThrowIfNull(avNumber);

            var sample = await _sampleRepository.GetSampleAsync(avNumber, sampleId);
            return _mapper.Map<SampleDto>(sample);
        }

        public async Task AddSample(SampleDto SampleDto, string avNumber, string userName)
        {
            ArgumentNullException.ThrowIfNull(SampleDto);

            var sample = _mapper.Map<Sample>(SampleDto);
            await _sampleRepository.AddSampleAsync(sample, avNumber, userName);
        }

        public async Task UpdateSample(SampleDto SampleDto, string userName)
        {
            ArgumentNullException.ThrowIfNull(SampleDto);

            var sample = _mapper.Map<Sample>(SampleDto);
            await _sampleRepository.UpdateSampleAsync(sample, userName);
        }

        public async Task DeleteSampleAsync(Guid sampleId, string userId, byte[] lastModified)
        {            
            ArgumentNullException.ThrowIfNull(sampleId);
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(lastModified);
            await _sampleRepository.DeleteSampleAsync(sampleId, userId, lastModified);
        }
    }
}

