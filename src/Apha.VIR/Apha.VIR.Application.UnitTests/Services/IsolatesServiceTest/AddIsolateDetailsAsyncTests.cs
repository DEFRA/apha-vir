using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute.ExceptionExtensions;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.IsolatesServiceTest
{
    public class AddIsolateDetailsAsyncTests
    {
        private readonly IIsolateRepository _mockIsolateRepository;
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ICharacteristicRepository _mockCharacteristicRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolatesService _mockIsolatesService;

        public AddIsolateDetailsAsyncTests()
        {
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockSubmissionRepository = Substitute.For<ISubmissionRepository>();
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockCharacteristicRepository = Substitute.For<ICharacteristicRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockIsolatesService = new IsolatesService(_mockIsolateRepository, _mockSubmissionRepository, _mockSampleRepository, _mockCharacteristicRepository, _mockMapper);
        }

        [Fact]
        public async Task Test_AddIsolateDetailsAsync_Success()
        {
            // Arrange
            var isolateDto = new IsolateDTO { IsolateId = Guid.NewGuid() };
            var isolateEntity = new Isolate();
            _mockMapper.Map<Isolate>(isolateDto).Returns(isolateEntity);
            _mockIsolateRepository.AddIsolateDetailsAsync(isolateEntity).Returns(isolateDto.IsolateId);

            // Act
            var result = await _mockIsolatesService.AddIsolateDetailsAsync(isolateDto);

            // Assert
            Assert.Equal(isolateDto.IsolateId, result);
            await _mockIsolateRepository.Received(1).AddIsolateDetailsAsync(isolateEntity);
        }        

        [Fact]
        public async Task Test_AddIsolateDetailsAsync_RepositoryException()
        {
            // Arrange
            var isolateDto = new IsolateDTO();
            var isolateEntity = new Isolate();
            _mockMapper.Map<Isolate>(isolateDto).Returns(isolateEntity);
            _mockIsolateRepository.AddIsolateDetailsAsync(isolateEntity).Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockIsolatesService.AddIsolateDetailsAsync(isolateDto));
        }
    }
}
