using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Apha.VIR.DataAccess.UnitTests.Repository.SubmissionRepositoryTest
{

    public class TestSubmissionRepository : SubmissionRepository
    {
        public bool InsertCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        private readonly IQueryable<int> _fakeCounts;
        private readonly IQueryable<string> _fakeStrings;
        private readonly Dictionary<string, Submission> _fakeDatabase = new Dictionary<string, Submission>();

        public TestSubmissionRepository(
            VIRDbContext context,

            IQueryable<int> fakeCounts,
            IQueryable<string> fakeStrings) : base(context)
        {
            _fakeCounts = fakeCounts;
            _fakeStrings = fakeStrings;
        }
        public void AddFakeSubmission(Submission submission)
        {
            _fakeDatabase[submission.Avnumber] = submission;
        }

        public override async Task<Submission> GetSubmissionDetailsByAVNumberAsync(string avNumber)
        {
            // Simulate some async operation
            await Task.Delay(1);


            if (_fakeDatabase.TryGetValue(avNumber, out var sub))
            {
                return sub; 
            }
            else
            {
                return new Submission
                {
                   Avnumber = avNumber,
                   SenderAddress = null,
                };

            }


        }

        protected override async Task<Submission> GetSubmissionDetail(SqlDataReader reader)
        {
            // Your implementation here
            return await Task.FromResult(new Submission
            {
                SubmissionId = Guid.NewGuid(),
                Avnumber = "FAKE123",
                CountryOfOrigin=Guid.NewGuid(),
                SenderAddress="aaa",

            });
        }


        protected override IQueryable<T> SqlQueryInterpolatedFor<T>(FormattableString sql)
        {
            if (typeof(T) == typeof(int))
                return (IQueryable<T>)_fakeCounts;
            if (typeof(T) == typeof(string))
                return (IQueryable<T>)_fakeStrings;

            throw new NotImplementedException($"No fake defined for type {typeof(T).Name}");
        }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            var query = sql.ToLowerInvariant();

            if (query.Contains("insert"))
            {
                InsertCalled = true;
                return Task.FromResult(1);
            }

            if (query.Contains("update"))
            {
                UpdateCalled = true;
                return Task.FromResult(1);
            }

            if (query.Contains("delete"))
            {
                DeleteCalled = true;
                return Task.FromResult(1);
            }

            throw new NotImplementedException($"No override for query {sql}");
        }
    }
    public class SubmissionRepositoryTests
    {
        private readonly TestSubmissionRepository _repo;
        private readonly Mock<VIRDbContext> _mockContext;

        private static readonly int[] CountGreaterThanZero = { 1 };
        private static readonly int[] CountZero = { 0 };
        private static readonly string[] LatestSubmissionStrings = { "AV001", "AV002" };
        private readonly IQueryable<int> _fakeCounts;
        private readonly IQueryable<string> _fakeStrings;
        public SubmissionRepositoryTests()
        {
            _mockContext = new Mock<VIRDbContext>();

            _fakeCounts = new List<int> { 1, 2, 3 }.AsQueryable();
            _fakeStrings = new List<string> { "test1", "test2" }.AsQueryable();
            _repo = new TestSubmissionRepository(
           _mockContext.Object,

           _fakeCounts,
           _fakeStrings
       );
        }
        [Fact]
        public async Task AVNumberExistsInVirAsync_ReturnsTrue_WhenCountGreaterThanZero()
        {
            var fakeCounts = new TestAsyncEnumerable<int>(CountGreaterThanZero);

            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, fakeCounts, new TestAsyncEnumerable<string>(Array.Empty<string>()));

            var result = await repo.AVNumberExistsInVirAsync("AV123");

            Assert.True(result);
        }

        [Fact]
        public async Task AVNumberExistsInVirAsync_ReturnsFalse_WhenCountZero()
        {
            var fakeCounts = new TestAsyncEnumerable<int>(CountZero);

            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, fakeCounts, new TestAsyncEnumerable<string>(Array.Empty<string>()));

            var result = await repo.AVNumberExistsInVirAsync("AV123");

            Assert.False(result);
        }

        [Fact]
        public async Task GetLatestSubmissionsAsync_ReturnsStrings()
        {
            var fakeStrings = new TestAsyncEnumerable<string>(LatestSubmissionStrings);

            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, new TestAsyncEnumerable<int>(Array.Empty<int>()), fakeStrings);

            var result = await repo.GetLatestSubmissionsAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains("AV001", result);
            Assert.Contains("AV002", result);
        }

        [Fact]
        public async Task AddSubmissionAsync_CallsExecuteSqlAsync()
        {

            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, new TestAsyncEnumerable<int>(Array.Empty<int>()), new TestAsyncEnumerable<string>(Array.Empty<string>()));
            var submission = new Submission { Avnumber = "AV123" };

            await repo.AddSubmissionAsync(submission, "user");

            Assert.True(repo.InsertCalled);
        }

        [Fact]
        public async Task UpdateSubmissionAsync_CallsExecuteSqlAsync()
        {

            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, new TestAsyncEnumerable<int>(Array.Empty<int>()), new TestAsyncEnumerable<string>(Array.Empty<string>()));
            var submission = new Submission { SubmissionId = Guid.NewGuid(), Avnumber = "AV123", LastModified = new byte[8] };

            await repo.UpdateSubmissionAsync(submission, "user");

            Assert.True(repo.UpdateCalled);
        }

        [Fact]
        public async Task DeleteSubmissionAsync_CallsExecuteSqlAsync()
        {
            // Arrange

            var repo = new TestSubmissionRepository(
                new Mock<VIRDbContext>().Object,



                new TestAsyncEnumerable<int>(Array.Empty<int>()),
                new TestAsyncEnumerable<string>(Array.Empty<string>()));

            var submissionId = Guid.NewGuid();
            var userId = "user123";
            var lastModified = new byte[8];

            // Act
            await repo.DeleteSubmissionAsync(submissionId, userId, lastModified);

            // Assert
            Assert.True(repo.DeleteCalled);
        }


        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {

            Assert.Throws<ArgumentNullException>(() => new SubmissionRepository(null!
));
        }
   
        [Fact]
        public async Task GetSubmissionDetailsByAVNumberAsync_ReturnsSubmission_WhenAVNumberExists()
        {
            // Arrange
            var expectedSubmission = new Submission
            {
                SubmissionId = Guid.NewGuid(),
                Avnumber = "AV123",
                // ... set other properties
            };
            _repo.AddFakeSubmission(expectedSubmission);

            // Act
            var result = await _repo.GetSubmissionDetailsByAVNumberAsync("AV123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSubmission.SubmissionId, result.SubmissionId);
            Assert.Equal(expectedSubmission.Avnumber, result.Avnumber);
            // ... assert other properties
        }

      

        [Fact]
        public async Task GetSubmissionDetailsByAVNumberAsync_HandlesNullValues_Correctly()
        {
            // Arrange
            var submissionWithNulls = new Submission
            {
                SubmissionId = Guid.NewGuid(),
                Avnumber = "AV456",
                // ... set some properties to null
            };
            _repo.AddFakeSubmission(submissionWithNulls);

            // Act
            var result = await _repo.GetSubmissionDetailsByAVNumberAsync("AV456");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(submissionWithNulls.SubmissionId, result.SubmissionId);
            Assert.Equal(submissionWithNulls.Avnumber, result.Avnumber);
            // ... assert that null properties are indeed null
        }
        [Fact]
        public async Task GetSubmissionDetailsByAVNumberAsync_ReturnsFullyPopulatedSubmission()
        {
            // Arrange
            var fullSubmission = new Submission
            {
                SubmissionId = Guid.NewGuid(),
                Avnumber = "AV789",
                SendersReferenceNumber = "SRN123",
                RlreferenceNumber = "RLR456",
                SubmittingLab = Guid.NewGuid(),
                Sender = "John Doe",
                SenderOrganisation = "Test Org",
                SenderAddress = "123 Test St",
                CountryOfOrigin = Guid.NewGuid(),
                SubmittingCountry = Guid.NewGuid(),
                ReasonForSubmission = Guid.NewGuid(),
                DateSubmissionReceived = DateTime.Now,
                Cphnumber = "CPH123",
                Owner = "Jane Smith",
                SamplingLocationPremises = "Test Location",
                NumberOfSamples = 5,
                LastModified = new byte[] { 0x01, 0x02, 0x03 },
                CountryOfOriginName = "TestCountry",
                SubmittingCountryName = "SubmitCountry"
            };
            _repo.AddFakeSubmission(fullSubmission);

            // Act
            var result = await _repo.GetSubmissionDetailsByAVNumberAsync("AV789");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fullSubmission.SubmissionId, result.SubmissionId);
            Assert.Equal(fullSubmission.Avnumber, result.Avnumber);
            Assert.Equal(fullSubmission.SendersReferenceNumber, result.SendersReferenceNumber);
            Assert.Equal(fullSubmission.RlreferenceNumber, result.RlreferenceNumber);
            Assert.Equal(fullSubmission.SubmittingLab, result.SubmittingLab);
            Assert.Equal(fullSubmission.Sender, result.Sender);
            Assert.Equal(fullSubmission.SenderOrganisation, result.SenderOrganisation);
            Assert.Equal(fullSubmission.SenderAddress, result.SenderAddress);
            Assert.Equal(fullSubmission.CountryOfOrigin, result.CountryOfOrigin);
            Assert.Equal(fullSubmission.SubmittingCountry, result.SubmittingCountry);
            Assert.Equal(fullSubmission.ReasonForSubmission, result.ReasonForSubmission);
            Assert.Equal(fullSubmission.DateSubmissionReceived, result.DateSubmissionReceived);
            Assert.Equal(fullSubmission.Cphnumber, result.Cphnumber);
            Assert.Equal(fullSubmission.Owner, result.Owner);
            Assert.Equal(fullSubmission.SamplingLocationPremises, result.SamplingLocationPremises);
            Assert.Equal(fullSubmission.NumberOfSamples, result.NumberOfSamples);
            Assert.Equal(fullSubmission.LastModified, result.LastModified);
            Assert.Equal(fullSubmission.CountryOfOriginName, result.CountryOfOriginName);
            Assert.Equal(fullSubmission.SubmittingCountryName, result.SubmittingCountryName);
        }
        [Fact]
        public async Task GetSubmissionDetailsByAVNumberAsync_ReturnsNewSubmission_WhenAVNumberDoesNotExist()
        {
            // Arrange
            string nonExistentAVNumber = "NONEXISTENT";

            // Act
            var result = await _repo.GetSubmissionDetailsByAVNumberAsync(nonExistentAVNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nonExistentAVNumber, result.Avnumber);
            Assert.Null(result.SenderAddress);
        }
       
        [Fact]
        public async Task GetSubmissionDetailsByAVNumberAsync_HandlesEmptyAVNumber()
        {
            // Arrange
            string emptyAVNumber = string.Empty;

            // Act
            var result = await _repo.GetSubmissionDetailsByAVNumberAsync(emptyAVNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(emptyAVNumber, result.Avnumber);
        }

    }

}
