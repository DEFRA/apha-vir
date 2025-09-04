using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SubmissionControllerTest
{
    public class SubmissionControllerTests
    {
        private readonly SubmissionController _controller;
        private readonly ISubmissionService _submissionService;
        private readonly IMapper _mapper;
        private readonly ILookupService _lookupService;
        private readonly ISenderService _senderService;

        public SubmissionControllerTests()
        {
            _submissionService = Substitute.For<ISubmissionService>();
            _lookupService = Substitute.For<ILookupService>();
            _senderService = Substitute.For<ISenderService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SubmissionController(_lookupService, _senderService, _submissionService, _mapper);
        }

        [Fact]
        public async Task SubmissionLetter_ReturnsViewResult_WithSubmissionLetterViewModel()
        {
            // Arrange
            string avNumber = "TEST123";
            string expectedLetterContent = "Test letter content";
            _submissionService.SubmissionLetter(avNumber, "TestUser").Returns(expectedLetterContent);

            // Act
            var result = await _controller.SubmissionLetter(avNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SubmissionLetterViewModel>(viewResult.Model);
            Assert.Equal(expectedLetterContent, model.LetterContent);
        }

        [Fact]
        public async Task SubmissionLetter_ThrowsException_WhenServiceFails()
        {
            // Arrange
            string avNumber = "TEST123";
            _submissionService.SubmissionLetter(avNumber, "TestUser").Returns(Task.FromException<string>(new Exception("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SubmissionLetter(avNumber));
        }
    }
}
