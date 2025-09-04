using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.SubmissionServiceTest
{
    public class SubmissionServiceTests
    {
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly IMapper _mockMapper;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly IIsolateRepository _mockIsolatesRepository;
        private readonly SubmissionService _submissionService;

        public SubmissionServiceTests()
        {
            _mockSubmissionRepository = Substitute.For<ISubmissionRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockIsolatesRepository = Substitute.For<IIsolateRepository>();
            _submissionService = new SubmissionService(_mockSubmissionRepository, _mockSampleRepository, _mockIsolatesRepository, _mockMapper);
        }

        [Fact]
        public async Task AVNumberExistsInVirAsync_ValidAVNumber_ReturnsTrue()
        {
            // Arrange
            string validAVNumber = "AV123456";
            _mockSubmissionRepository.AVNumberExistsInVirAsync(validAVNumber).Returns(true);

            // Act
            var result = await _submissionService.AVNumberExistsInVirAsync(validAVNumber);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AVNumberExistsInVirAsync_InvalidAVNumber_ReturnsFalse()
        {
            // Arrange
            string invalidAVNumber = "AV999999";
            _mockSubmissionRepository.AVNumberExistsInVirAsync(invalidAVNumber).Returns(false);

            // Act
            var result = await _submissionService.AVNumberExistsInVirAsync(invalidAVNumber);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task AVNumberExistsInVirAsync_EmptyOrNullAVNumber_ReturnsFalse(string avNumber)
        {
            // Arrange
            _mockSubmissionRepository.AVNumberExistsInVirAsync(avNumber).Returns(false);

            // Act
            var result = await _submissionService.AVNumberExistsInVirAsync(avNumber);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AVNumberExistsInVirAsync_ExceptionThrown_ThrowsException()
        {
            // Arrange
            string avNumber = "AV123456";
            _mockSubmissionRepository.AVNumberExistsInVirAsync(avNumber).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _submissionService.AVNumberExistsInVirAsync(avNumber));
        }

        [Fact]
        public async Task GetSubmissionDetailsByAVNumberAsync_ExistingAVNumber_ReturnsSubmissionDTO()
        {
            // Arrange
            var avNumber = "AV123";
            var submission = new Submission { Avnumber = avNumber };
            var expectedDto = new SubmissionDTO { Avnumber = avNumber };

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockMapper.Map<SubmissionDTO>(submission).Returns(expectedDto);

            // Act
            var result = await _submissionService.GetSubmissionDetailsByAVNumberAsync(avNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto, result);
        }

        [Fact]
        public async Task GetSubmissionDetailsByAVNumberAsync_NonExistentAVNumber_ReturnsNull()
        {
            // Arrange
            var avNumber = "NonExistent";
            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns((Submission)null!);

            // Act
            var result = await _submissionService.GetSubmissionDetailsByAVNumberAsync(avNumber);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSubmissionDetailsByAVNumberAsync_ValidSubmission_MapsCorrectly()
        {
            // Arrange
            var avNumber = "AV456";
            var submission = new Submission { Avnumber = avNumber, Sender = "Test Submission" };
            var expectedDto = new SubmissionDTO { Avnumber = avNumber, Sender = "Test Submission" };

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockMapper.Map<SubmissionDTO>(submission).Returns(expectedDto);

            // Act
            var result = await _submissionService.GetSubmissionDetailsByAVNumberAsync(avNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Avnumber, result.Avnumber);
            Assert.Equal(expectedDto.Sender, result.Sender);
            _mockMapper.Received(1).Map<SubmissionDTO>(submission);
        }

        [Fact]
        public async Task AddSubmissionAsync_ValidSubmission_SuccessfullyAdded()
        {
            // Arrange
            var submissionDto = new SubmissionDTO { Avnumber = "AV123" };
            var submissionEntity = new Submission { Avnumber = "AV123" };
            _mockMapper.Map<Submission>(submissionDto).Returns(submissionEntity);

            // Act
            await _submissionService.AddSubmissionAsync(submissionDto, "testUser");

            // Assert
            await _mockSubmissionRepository.Received(1).AddSubmissionAsync(Arg.Is<Submission>(s => s.Avnumber == "AV123"), "testUser");
        }

        [Fact]
        public async Task AddSubmissionAsync_RepositoryThrowsException_ExceptionPropagated()
        {
            // Arrange
            var submissionDto = new SubmissionDTO { Avnumber = "AV123" };
            var submissionEntity = new Submission { Avnumber = "AV123" };
            _mockMapper.Map<Submission>(submissionDto).Returns(submissionEntity);
            _mockSubmissionRepository.AddSubmissionAsync(Arg.Any<Submission>(), Arg.Any<string>()).Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _submissionService.AddSubmissionAsync(submissionDto, "testUser"));
        }

        [Fact]
        public async Task UpdateSubmissionAsync_ValidSubmission_CallsRepositoryUpdate()
        {
            // Arrange
            var submissionDto = new SubmissionDTO { SubmissionId = Guid.NewGuid(), Avnumber = "AV001" };
            var submissionEntity = new Submission { SubmissionId = Guid.NewGuid(), Avnumber = "AV001" };
            _mockMapper.Map<Submission>(submissionDto).Returns(submissionEntity);

            // Act
            await _submissionService.UpdateSubmissionAsync(submissionDto, "testUser");

            // Assert
            await _mockSubmissionRepository.Received(1).UpdateSubmissionAsync(Arg.Is<Submission>(s => s == submissionEntity), Arg.Is<string>(u => u == "testUser"));
        }

        [Fact]
        public async Task UpdateSubmissionAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var submissionDto = new SubmissionDTO { SubmissionId = Guid.NewGuid(), Avnumber = "AV001" };
            var submissionEntity = new Submission { SubmissionId = Guid.NewGuid(), Avnumber = "AV001" };
            _mockMapper.Map<Submission>(submissionDto).Returns(submissionEntity);
            _mockSubmissionRepository.UpdateSubmissionAsync(Arg.Any<Submission>(), Arg.Any<string>())
            .Returns(Task.FromException(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _submissionService.UpdateSubmissionAsync(submissionDto, "testUser"));
        }

       [Fact]
        public async Task SubmissionLetter_ShouldReturnExpectedResult()
        {
            // Arrange
            var avNumber = "AV123";
            var user = "TestUser";
            var submission = new Submission
            {
                SubmissionId = Guid.NewGuid(),
                Avnumber = avNumber,
                Sender = "John Doe",
                SenderOrganisation = "Test Org",
                SenderAddress = "123 Test St",
                SubmittingCountryName = "Test Country",
                SendersReferenceNumber = "REF123",
                DateSubmissionReceived = DateTime.Now.AddDays(-7),
                CountryOfOriginName = "Origin Country"
            };

            var sampleId = Guid.NewGuid();

            var samples = new List<Sample>
            {
            new Sample { SampleId = sampleId, SenderReferenceNumber = "S1", HostSpeciesName = "Chicken" }
            };

            var isolates = new List<IsolateInfo>
            {
            new IsolateInfo { IsolateSampleId = sampleId, YearOfIsolation = 2023 }
            };

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(samples);
            _mockIsolatesRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolates);

            // Act
            var result = await _submissionService.SubmissionLetter(avNumber, user);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(avNumber, result);
            Assert.Contains(user, result);
            Assert.Contains(submission.Sender, result);
            Assert.Contains(submission.SenderOrganisation, result);
            Assert.Contains(submission.SenderAddress, result);
            Assert.Contains(submission.SubmittingCountryName, result);
            Assert.Contains(submission.SendersReferenceNumber, result);
            Assert.Contains(submission.CountryOfOriginName, result);
            Assert.Contains(samples[0].SenderReferenceNumber, result);
            Assert.Contains(samples[0].HostSpeciesName, result);
            Assert.Contains(isolates[0].YearOfIsolation.ToString(), result);
        }

        [Fact]
        public async Task SubmissionLetter_WithMissingData_ShouldReturnExpectedResult()
        {
            // Arrange
            var avNumber = "AV456";
            var user = "TestUser";
            var submission = new Submission
            {
                SubmissionId = Guid.NewGuid(),
                Avnumber = avNumber,
                Sender = "Jane Doe",
                SenderOrganisation = "Test Org 2",
                SenderAddress = "456 Test Ave",
                SubmittingCountryName = "Test Country 2",
                SendersReferenceNumber = "REF456",
                DateSubmissionReceived = null,
                CountryOfOriginName = null
            };

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(new List<Sample>());
            _mockIsolatesRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfo>());

            // Act
            var result = await _submissionService.SubmissionLetter(avNumber, user);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(avNumber, result);
            Assert.Contains(user, result);
            Assert.Contains(submission.Sender, result);
            Assert.Contains(submission.SenderOrganisation, result);
            Assert.Contains(submission.SenderAddress, result);
            Assert.Contains(submission.SubmittingCountryName, result);
            Assert.Contains(submission.SendersReferenceNumber, result);
            Assert.Contains("[Missing]", result);
            Assert.Contains("Date of Receipt:", result);
        }
    }
}
