using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Microsoft.Data.SqlClient;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.SampleRepositoryTest
{
    public class TestSampleRepository : SampleRepository
    {
        private readonly IQueryable<Sample> _samples;
        private readonly IQueryable<Submission> _submissions;
        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public object[]? LastAddParameters { get; private set; }
        public object[]? LastUpdateParameters { get; private set; }
        public bool DeleteCalled { get; private set; }
        public object[]? LastDeleteParameters { get; private set; }

        public TestSampleRepository(VIRDbContext context, IQueryable<Sample> samples, IQueryable<Submission> submissions)
            : base(context)
        {
            _samples = samples;
            _submissions = submissions;
        }

        protected override IQueryable<T> GetQueryableInterpolatedFor<T>(FormattableString sql)
        {
            if (typeof(T) == typeof(Sample))
                return (IQueryable<T>)_samples;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }
        protected override IQueryable<T> GetDbSetFor<T>()
        {
            if (typeof(T) == typeof(Sample))
                return new TestAsyncEnumerable<T>(_samples.AsQueryable().Expression);
            if (typeof(T) == typeof(Submission))
                return new TestAsyncEnumerable<T>(_submissions.AsQueryable().Expression);
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }


        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            if (sql.Contains("spSampleInsert"))
            {
                AddCalled = true;
                LastAddParameters = parameters;
                return Task.FromResult(1);
            }
            if (sql.Contains("spSampleUpdate"))
            {
                UpdateCalled = true;
                LastUpdateParameters = parameters;
                return Task.FromResult(1);
            }
            if (sql.Contains("spSampleDelete"))
            {
                DeleteCalled = true;
                LastDeleteParameters = parameters;
                return Task.FromResult(1);
            }
            throw new NotImplementedException($"No override for query {sql}");
        }
    }
    public class SampleRepositoryTests
    {
        [Fact]
        public async Task GetSamplesBySubmissionIdAsync_ReturnsSamples()
        {
            var submissionId = Guid.NewGuid();
            var samples = new List<Sample>
            {
                new Sample { SampleSubmissionId = submissionId, SampleNumber = 1 },
                new Sample { SampleSubmissionId = submissionId, SampleNumber = 2 }
            };
            var asyncSamples = new TestAsyncEnumerable<Sample>(samples);
            var repo = new TestSampleRepository(new Mock<VIRDbContext>().Object, asyncSamples, new TestAsyncEnumerable<Submission>(Enumerable.Empty<Submission>()));

            var result = await repo.GetSamplesBySubmissionIdAsync(submissionId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetSampleAsync_ReturnsNull_WhenAvNumberIsNullOrSampleIdIsNull()
        {
            var repo = new TestSampleRepository(new Mock<VIRDbContext>().Object,
                new TestAsyncEnumerable<Sample>(Enumerable.Empty<Sample>()),
                new TestAsyncEnumerable<Submission>(Enumerable.Empty<Submission>()));

            var result1 = await repo.GetSampleAsync(string.Empty, Guid.NewGuid()); // Replace null with string.Empty
            var result2 = await repo.GetSampleAsync("AV123", null); // Replace null with Guid.Empty

            Assert.Null(result1);
            Assert.Null(result2);
        }

        [Fact]
        public async Task GetSampleAsync_ReturnsNull_WhenSubmissionNotFound()
        {
            var repo = new TestSampleRepository(new Mock<VIRDbContext>().Object,
                new TestAsyncEnumerable<Sample>(Enumerable.Empty<Sample>()),
                new TestAsyncEnumerable<Submission>(Enumerable.Empty<Submission>()));

            var result = await repo.GetSampleAsync("AV123", Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetSampleAsync_ReturnsSample_WhenFound()
        {
            var submissionId = Guid.NewGuid();
            var sampleId = Guid.NewGuid();
            var submissions = new List<Submission>
            {
                new Submission { SubmissionId = submissionId, Avnumber = "AV123" }
            };
            var samples = new List<Sample>
            {
                new Sample { SampleSubmissionId = submissionId, SampleId = sampleId }
            };
            var repo = new TestSampleRepository(
                new Mock<VIRDbContext>().Object,
                new TestAsyncEnumerable<Sample>(samples),
                new TestAsyncEnumerable<Submission>(submissions)
            );

            var result = await repo.GetSampleAsync("AV123", sampleId);

            Assert.NotNull(result);
            Assert.Equal(sampleId, result.SampleId);
        }

        [Fact]
        public async Task AddSampleAsync_CallsExecuteSqlAsync()
        {
            var submissionId = Guid.NewGuid();
            var submissions = new List<Submission>
            {
                new Submission { SubmissionId = submissionId, Avnumber = "AV123" }
            };
            var samples = new List<Sample>
            {
                new Sample { SampleSubmissionId = submissionId, SampleNumber = 1 }
            };
            var repo = new TestSampleRepository(
                new Mock<VIRDbContext>().Object,
                new TestAsyncEnumerable<Sample>(samples),
                new TestAsyncEnumerable<Submission>(submissions)
            );

            var sample = new Sample
            {
                SMSReferenceNumber = "SMS",
                SenderReferenceNumber = "Sender",
                SampleType = Guid.NewGuid(),
                HostSpecies = Guid.NewGuid(),
                HostBreed = Guid.NewGuid(),
                HostPurpose = Guid.NewGuid(),
                SamplingLocationHouse = "House",
                LastModified = new byte[8]
            };

            await repo.AddSampleAsync(sample, "AV123", "User1");

            Assert.True(repo.AddCalled);
            Assert.NotNull(repo.LastAddParameters);
            Assert.Contains(repo.LastAddParameters, p => p is Microsoft.Data.SqlClient.SqlParameter);
        }

        [Fact]
        public async Task UpdateSampleAsync_CallsExecuteSqlAsync()
        {
            var repo = new TestSampleRepository(
                new Mock<VIRDbContext>().Object,
                new TestAsyncEnumerable<Sample>(Enumerable.Empty<Sample>()),
                new TestAsyncEnumerable<Submission>(Enumerable.Empty<Submission>())
            );

            var sample = new Sample
            {
                SampleId = Guid.NewGuid(),
                SampleSubmissionId = Guid.NewGuid(),
                SampleNumber = 1,
                SMSReferenceNumber = "SMS",
                SenderReferenceNumber = "Sender",
                SampleType = Guid.NewGuid(),
                HostSpecies = Guid.NewGuid(),
                HostBreed = Guid.NewGuid(),
                HostPurpose = Guid.NewGuid(),
                SamplingLocationHouse = "House",
                LastModified = new byte[8]
            };

            await repo.UpdateSampleAsync(sample, "User1");

            Assert.True(repo.UpdateCalled);
            Assert.NotNull(repo.LastUpdateParameters);
            Assert.Contains(repo.LastUpdateParameters, p => p is Microsoft.Data.SqlClient.SqlParameter);
        }
        [Fact]
        public async Task DeleteSampleAsync_CallsExecuteSqlAsync()
        {
            // Arrange
            var repo = new TestSampleRepository(
                new Mock<VIRDbContext>().Object,
                new TestAsyncEnumerable<Sample>(Enumerable.Empty<Sample>()),
                new TestAsyncEnumerable<Submission>(Enumerable.Empty<Submission>())
            );

            var sampleId = Guid.NewGuid();
            var userId = "User1";
            var lastModified = new byte[8];

            // Act
            await repo.DeleteSampleAsync(sampleId, userId, lastModified);

            // Assert
            Assert.True(repo.DeleteCalled);
            Assert.NotNull(repo.LastDeleteParameters);
            Assert.Equal(3, repo.LastDeleteParameters.Length);
            Assert.Contains(repo.LastDeleteParameters, p => p is SqlParameter sqlParam && sqlParam.ParameterName == "@UserID" && (string)sqlParam.Value == userId);
            Assert.Contains(repo.LastDeleteParameters, p => p is SqlParameter sqlParam && sqlParam.ParameterName == "@SampleId" && (Guid)sqlParam.Value == sampleId);
            Assert.Contains(repo.LastDeleteParameters, p => p is SqlParameter sqlParam && sqlParam.ParameterName == "@LastModified" && (byte[])sqlParam.Value == lastModified);
        }
      
      

        [Fact]
        public async Task DeleteSampleAsync_WithValidInput_DoesNotThrowException()
        {
            // Arrange
            var repo = new TestSampleRepository(
                new Mock<VIRDbContext>().Object,
                new TestAsyncEnumerable<Sample>(Enumerable.Empty<Sample>()),
                new TestAsyncEnumerable<Submission>(Enumerable.Empty<Submission>())
            );

            var sampleId = Guid.NewGuid();
            var userId = "User1";
            var lastModified = new byte[8];

            // Act & Assert
            var exception = await Record.ExceptionAsync(() =>
                repo.DeleteSampleAsync(sampleId, userId, lastModified));
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SampleRepository(null!));
        }
    }
}
