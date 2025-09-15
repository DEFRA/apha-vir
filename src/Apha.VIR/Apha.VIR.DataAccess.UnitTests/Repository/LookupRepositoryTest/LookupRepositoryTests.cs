using System.Security;
using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Microsoft.Data.SqlClient;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.LookupRepositoryTest
{
    public class TestLookupRepository : LookupRepository
    {
        private readonly IQueryable<LookupItem> _lookupItems;
        private readonly IQueryable<Lookup>? _lookups;
        public Func<string, object[], Task<int>>? ExecuteSqlAsyncOverride { get; set; }

        public TestLookupRepository(VIRDbContext context, IQueryable<LookupItem> lookupItems)
            : base(context)
        {
            _lookupItems = lookupItems;
        }
        // New constructor for Lookup tests
        public TestLookupRepository(VIRDbContext context, IQueryable<LookupItem> lookupItems, IQueryable<Lookup> lookups)
            : base(context)
        {
            _lookupItems = lookupItems;
            _lookups = lookups;
        }


        protected override IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(LookupItem))
                return (IQueryable<T>)_lookupItems;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }
        protected override IQueryable<T> GetDbSetFor<T>()
        {
            if (typeof(T) == typeof(Lookup) && _lookups != null)
                return (IQueryable<T>)_lookups;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }
        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            if (ExecuteSqlAsyncOverride != null)
                return ExecuteSqlAsyncOverride(sql, parameters);
            return Task.FromResult(1);
        }
    }

    public class LookupRepositoryTests
    {
        private static TestLookupRepository CreateRepo(IEnumerable<LookupItem> lookupItems)
        {
            var mockContext = new Mock<VIRDbContext>();
            return new TestLookupRepository(
                mockContext.Object,
                new TestAsyncEnumerable<LookupItem>(lookupItems)
            );
        }
        private static TestLookupRepository CreateRepoT(IEnumerable<LookupItem> lookupItems, IEnumerable<Lookup> lookups)
        {
            var mockContext = new Mock<VIRDbContext>();
            return new TestLookupRepository(
                mockContext.Object,
                new TestAsyncEnumerable<LookupItem>(lookupItems),
                new TestAsyncEnumerable<Lookup>(lookups)
            );
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllVirusFamiliesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllVirusTypesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllVirusTypesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllHostSpeciesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllHostBreedsAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllHostBreedsAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllHostBreedsAltNameAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllHostBreedsAltNameAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllCountriesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllCountriesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllHostPurposesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllHostPurposesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllSampleTypesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllSampleTypesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllWorkGroupsAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllStaffAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllStaffAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllViabilityAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllViabilityAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllSubmittingLabAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllSubmissionReasonAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllSubmissionReasonAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllIsolationMethodsAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllIsolationMethodsAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllFreezerAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllFreezerAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllTraysAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllTraysAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task InsertLookupItemAsync_ThrowsArgumentException_WhenInsertCommandIsNullOrWhitespace()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup>
        {
            new() { Id = lookupId, InsertCommand = null! }
        };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            var item = new LookupItem { Name = "Test", Active = true };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.InsertLookupItemAsync(lookupId, item));
            Assert.Contains("Lookup Insert Stored procedure name is required", ex.Message);
        }        

        [Fact]
        public async Task InsertLookupItemAsync_CallsExecuteSqlAsync_WithCorrectParameters()
        {
            var lookupId = Guid.NewGuid();
            var allowedProc = "spInsertAllowed";
            var lookups = new List<Lookup>
        {
            new() { Id = lookupId, InsertCommand = allowedProc }
        };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            // Override ExecuteSqlAsync to verify call
            bool called = false;
            repo.ExecuteSqlAsyncOverride = (sql, parameters) =>
            {
                called = true;
                Assert.Contains(allowedProc, sql);
                Assert.NotNull(parameters);
                return Task.FromResult(1);
            };

            var item = new LookupItem
            {
                Name = "TestName",
                AlternateName = "AltName",
                Parent = Guid.NewGuid(),
                Active = true
            };

            await repo.InsertLookupItemAsync(lookupId, item);
            Assert.True(called);
        }

        [Fact]
        public async Task InsertLookupItemAsync_SetsNullsAndDefaults_WhenItemPropertiesAreNullOrEmpty()
        {
            var lookupId = Guid.NewGuid();
            var allowedProc = "spInsertAllowed";
            var lookups = new List<Lookup>
            {
                new() { Id = lookupId, InsertCommand = allowedProc }
            };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            bool called = false;
            repo.ExecuteSqlAsyncOverride = (sql, parameters) =>
            {
                called = true;
                // Check that DBNull is used for null/empty properties
                var nameParam = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@Name");
                var altNameParam = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@AltName");
                var parentParam = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@Parent");

                Assert.NotNull(nameParam);
                Assert.NotNull(altNameParam);
                Assert.NotNull(parentParam);

                Assert.Equal(DBNull.Value, nameParam.Value);
                Assert.Equal(DBNull.Value, altNameParam.Value);
                Assert.Equal(DBNull.Value, parentParam.Value);
                return Task.FromResult(1);
            };

            var item = new LookupItem
            {
                Name = null!,
                AlternateName = null,
                Parent = Guid.Empty,
                Active = false
            };

            await repo.InsertLookupItemAsync(lookupId, item);
            Assert.True(called);
        }

        [Fact]
        public async Task UpdateLookupItemAsync_ThrowsArgumentException_WhenUpdateCommandIsNullOrWhitespace()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup>
    {
        new() { Id = lookupId, UpdateCommand = null! }
    };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            var item = new LookupItem { Id = Guid.NewGuid(), Name = "Test", Active = true };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.UpdateLookupItemAsync(lookupId, item));
            Assert.Contains("Lookup update Stored procedure name is required", ex.Message);
        }

        [Fact]
        public async Task UpdateLookupItemAsync_CallsExecuteSqlAsync_WithCorrectParameters()
        {
            var lookupId = Guid.NewGuid();
            var allowedProc = "spUpdateAllowed";
            var lookups = new List<Lookup>
    {
        new() { Id = lookupId, UpdateCommand = allowedProc }
    };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            bool called = false;
            repo.ExecuteSqlAsyncOverride = (sql, parameters) =>
            {
                called = true;
                Assert.Contains(allowedProc, sql);
                Assert.NotNull(parameters);
                return Task.FromResult(1);
            };

            var item = new LookupItem
            {
                Id = Guid.NewGuid(),
                Name = "TestName",
                AlternateName = "AltName",
                Parent = Guid.NewGuid(),
                Active = true
            };

            await repo.UpdateLookupItemAsync(lookupId, item);
            Assert.True(called);
        }

        [Fact]
        public async Task UpdateLookupItemAsync_SetsNullsAndDefaults_WhenItemPropertiesAreNullOrEmpty()
        {
            var lookupId = Guid.NewGuid();
            var allowedProc = "spUpdateAllowed";
            var lookups = new List<Lookup>
    {
        new() { Id = lookupId, UpdateCommand = allowedProc }
    };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            bool called = false;
            repo.ExecuteSqlAsyncOverride = (sql, parameters) =>
            {
                called = true;
                var idParam = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@ID");
                var nameParam = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@Name");
                var altNameParam = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@AltName");
                var parentParam = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@Parent");

                Assert.NotNull(idParam);
                Assert.NotNull(nameParam);
                Assert.NotNull(altNameParam);
                Assert.NotNull(parentParam);

                Assert.Equal(DBNull.Value, idParam!.Value);
                Assert.Equal(DBNull.Value, nameParam!.Value);
                Assert.Equal(DBNull.Value, altNameParam!.Value);
                Assert.Equal(DBNull.Value, parentParam!.Value);
                return Task.FromResult(1);
            };

            var item = new LookupItem
            {
                Id = Guid.Empty,
                Name = null!,
                AlternateName = null,
                Parent = Guid.Empty,
                Active = false
            };

            await repo.UpdateLookupItemAsync(lookupId, item);
            Assert.True(called);
        }

        [Fact]
        public async Task DeleteLookupItemAsync_ThrowsArgumentException_WhenDeleteCommandIsNullOrWhitespace()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup>
    {
        new() { Id = lookupId, DeleteCommand = null! }
    };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            var item = new LookupItem { Id = Guid.NewGuid(), Name = "Test", Active = true };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.DeleteLookupItemAsync(lookupId, item));
            Assert.Contains("Lookup delete Stored procedure name is required", ex.Message);
        }        

        [Fact]
        public async Task DeleteLookupItemAsync_CallsExecuteSqlAsync_WithCorrectParameters()
        {
            var lookupId = Guid.NewGuid();
            var allowedProc = "spDeleteAllowed";
            var lookups = new List<Lookup>
    {
        new() { Id = lookupId, DeleteCommand = allowedProc }
    };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            bool called = false;
            repo.ExecuteSqlAsyncOverride = (sql, parameters) =>
            {
                called = true;
                Assert.Contains(allowedProc, sql);
                Assert.NotNull(parameters);
                return Task.FromResult(1);
            };

            var item = new LookupItem
            {
                Id = Guid.NewGuid(),
                Name = "TestName",
                AlternateName = "AltName",
                Parent = Guid.NewGuid(),
                Active = true
            };

            await repo.DeleteLookupItemAsync(lookupId, item);
            Assert.True(called);
        }

        [Fact]
        public async Task DeleteLookupItemAsync_SetsNullsAndDefaults_WhenItemPropertiesAreNullOrEmpty()
        {
            var lookupId = Guid.NewGuid();
            var allowedProc = "spDeleteAllowed";
            var lookups = new List<Lookup>
    {
        new() { Id = lookupId, DeleteCommand = allowedProc }
    };
            var repo = CreateRepoT(new List<LookupItem>(), lookups);

            bool called = false;
            repo.ExecuteSqlAsyncOverride = (sql, parameters) =>
            {
                called = true;
                var idParam = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@ID");
                Assert.NotNull(idParam);
                Assert.Equal(DBNull.Value, idParam!.Value);
                return Task.FromResult(1);
            };

            var item = new LookupItem
            {
                Id = Guid.Empty,
                Name = null!,
                AlternateName = null,
                Parent = Guid.Empty,
                Active = false
            };

            await repo.DeleteLookupItemAsync(lookupId, item);
            Assert.True(called);
        }


    }
}
