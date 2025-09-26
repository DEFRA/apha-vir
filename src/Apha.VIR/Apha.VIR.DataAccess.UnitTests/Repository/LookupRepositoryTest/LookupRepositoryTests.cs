using System.Reflection;
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
        protected override IQueryable<T> GetQueryableInterpolatedFor<T>(FormattableString sql)
        {
            if (typeof(T) == typeof(Lookup) && _lookups != null)
                return (IQueryable<T>)_lookups;
            if (typeof(T) == typeof(LookupItem))
                return (IQueryable<T>)_lookupItems;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }
        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            if (ExecuteSqlAsyncOverride != null)
                return ExecuteSqlAsyncOverride(sql, parameters);
            return Task.FromResult(1);
        }
    }

    public class TestLookupRepositoryWithByParent : LookupRepository
    {
        private readonly IEnumerable<LookupItem> _lookupItems;
        private readonly Func<string, Guid?, Task<IEnumerable<LookupItem>>>? _getLookupItemsByParentAsyncOverride;

        public TestLookupRepositoryWithByParent(
            VIRDbContext context,
            IEnumerable<LookupItem> lookupItems,
            Func<string, Guid?, Task<IEnumerable<LookupItem>>>? getLookupItemsByParentAsyncOverride = null)
            : base(context)
        {
            _lookupItems = lookupItems;
            _getLookupItemsByParentAsyncOverride = getLookupItemsByParentAsyncOverride;
        }

        protected override IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(LookupItem))
                return (IQueryable<T>)_lookupItems.AsQueryable();
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }

        // Override the private method with protected virtual to make it testable
        protected override async Task<IEnumerable<LookupItem>> GetLookupItemsByParentAsync(string Lookup, Guid? Parent)
        {
            if (_getLookupItemsByParentAsyncOverride != null)
                return await _getLookupItemsByParentAsyncOverride(Lookup, Parent);

            return _lookupItems.Where(i => i.Active).ToList();
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
        private static TestLookupRepositoryWithByParent CreateRepoWithByParent(
            IEnumerable<LookupItem> lookupItems,
            Func<string, Guid?, Task<IEnumerable<LookupItem>>>? getLookupItemsByParentAsyncOverride = null)
        {
            var mockContext = new Mock<VIRDbContext>();
            return new TestLookupRepositoryWithByParent(
                mockContext.Object,
                lookupItems,
                getLookupItemsByParentAsyncOverride
            );
        }
        private class TestLookupRepositoryForInUse : LookupRepository
        {
            private readonly IQueryable<Lookup> _lookups;
            public Func<string, object[], Task<int>>? ExecuteSqlAsyncOverride { get; set; }

            // Removed duplicate method definition
            protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
            {
                if (ExecuteSqlAsyncOverride != null)
                {
                    return ExecuteSqlAsyncOverride(sql, parameters);
                }
                return Task.FromResult(1);
            }

            // Updated constructor to match the usage in the code
            public TestLookupRepositoryForInUse(VIRDbContext context, IQueryable<Lookup> lookups, Func<string, object[], Task<int>>? execOverride = null)
                : base(context)
            {
                _lookups = lookups;
                ExecuteSqlAsyncOverride = execOverride;
            }

            protected override IQueryable<T> GetDbSetFor<T>()
            {
                if (typeof(T) == typeof(Lookup))
                    return (IQueryable<T>)_lookups;
                throw new NotImplementedException();
            }

            protected override IQueryable<T> GetQueryableInterpolatedFor<T>(FormattableString sql)
            {
                if (typeof(T) == typeof(Lookup))
                    return (IQueryable<T>)_lookups;
                throw new NotImplementedException($"No test data for type {typeof(T).Name}");
            }
        }

        private static TestLookupRepositoryForInUse CreateRepoForInUse(IEnumerable<Lookup> lookups, Func<string, object[], Task<int>>? execOverride = null)
        {
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestLookupRepositoryForInUse(mockContext.Object, new TestAsyncEnumerable<Lookup>(lookups));
            if (execOverride != null)
                repo.ExecuteSqlAsyncOverride = execOverride;
            return repo;
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

        [Fact]
        public async Task IsLookupItemInUseAsync_ReturnsFalse_WhenNoLookupFound()
        {
            var repo = CreateRepoForInUse(Enumerable.Empty<Lookup>());
            var result = await repo.IsLookupItemInUseAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.False(result);
        }

        [Fact]
        public async Task IsLookupItemInUseAsync_ReturnsFalse_WhenNoExceptionThrown()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup>
        {
            new() { Id = lookupId, InUseCommand = "spInUse" }
        };
            bool called = false;
            var repo = CreateRepoForInUse(lookups, (sql, parameters) =>
            {
                called = true;
                Assert.Contains("spInUse", sql);
                Assert.NotNull(parameters);
                var param = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@ID");
                Assert.NotNull(param);
                return (Task<int>)Task.CompletedTask;
            });

            var result = await repo.IsLookupItemInUseAsync(lookupId, Guid.NewGuid());
            Assert.True(called);
            Assert.False(result);
        }       

        [Fact]
        public async Task IsLookupItemInUseAsync_PassesDBNull_WhenLookupItemIdIsEmpty()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup>
        {
            new() { Id = lookupId, InUseCommand = "spInUse" }
        };
            bool called = false;
            var repo = CreateRepoForInUse(lookups, (sql, parameters) =>
            {
                called = true;
                var param = parameters.OfType<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@ID");
                Assert.NotNull(param);
                Assert.Equal(DBNull.Value, param!.Value);
                return (Task<int>)Task.CompletedTask;
            });

            var result = await repo.IsLookupItemInUseAsync(lookupId, Guid.Empty);
            Assert.True(called);
            Assert.False(result);
        }

        [Fact]
        public async Task GetLookupItemAsync_ReturnsNewItem_WhenLookupNull()
        {
            var repo = CreateRepoT([], []);
            var result = await repo.GetLookupItemAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task GetAllLookupItemsAsync_Paged_ReturnsEmpty_WhenLookupNull()
        {
            var repo = CreateRepoT([], []);
            var result = await repo.GetAllLookupItemsAsync(Guid.NewGuid(), 1, 2);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetAllLookupItemsAsync_NoPaging_ReturnsEmpty_WhenLookupNull()
        {
            var repo = CreateRepoT([], []);
            var result = await repo.GetAllLookupItemsAsync(Guid.NewGuid());
            Assert.Empty(result);
        }

        [Fact]
        public async Task IsLookupItemInUseAsync_ReturnsFalse_WhenLookupNull()
        {
            var repo = CreateRepoForInUse([]);
            var result = await repo.IsLookupItemInUseAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.False(result);
        }      

        [Fact]
        public async Task GetAllLookupsAsync_ReturnsLookups()
        {
            var lookups = new List<Lookup> { new() { Id = Guid.NewGuid() } };
            var repo = CreateRepoT([], lookups);
            var result = await repo.GetAllLookupsAsync();
            Assert.Single(result);
        }

        [Fact]
        public async Task GetLookupByIdAsync_ReturnsLookup_WhenFound()
        {
            var id = Guid.NewGuid();
            var lookups = new List<Lookup> { new() { Id = id } };
            var repo = CreateRepoT([], lookups);
            var result = await repo.GetLookupByIdAsync(id);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task GetLookupByIdAsync_ReturnsNewLookup_WhenNotFound()
        {
            var lookups = new List<Lookup>();
            var repo = CreateRepoT([], lookups);
            var result = await repo.GetLookupByIdAsync(Guid.NewGuid());
            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task GetLookupItemAsync_ReturnsItem_WhenFound()
        {
            var lookupId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var lookups = new List<Lookup> { new() { Id = lookupId, SelectCommand = "spTest" } };
            var items = new List<LookupItem> { new() { Id = itemId } };
            var repo = CreateRepoT(items, lookups);
            var result = await repo.GetLookupItemAsync(lookupId, itemId);
            Assert.Equal(itemId, result.Id);
        }

        [Fact]
        public async Task GetAllLookupItemsAsync_Paged_ReturnsData()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup> { new() { Id = lookupId, SelectCommand = "spTest" } };
            var items = Enumerable.Range(1, 5).Select(i => new LookupItem { Id = Guid.NewGuid() }).ToList();
            var repo = CreateRepoT(items, lookups);
            var result = await repo.GetAllLookupItemsAsync(lookupId, 1, 2);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(5, result.TotalCount);
        }

        [Fact]
        public async Task GetLookupItemParentListAsync_CallsUnderlying()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup> { new() { Id = lookupId, SelectCommand = "spTest" } };
            var items = new List<LookupItem> { new() { Id = Guid.NewGuid() } };
            var repo = CreateRepoT(items, lookups);
            var result = await repo.GetLookupItemParentListAsync(lookupId);
            Assert.Single(result);
        }


        [Fact]
        public async Task InsertLookupItemAsync_Executes_WhenAllowed()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup>
            {
                new() { Id = lookupId, InsertCommand = "spInsert" }
            };
            var repo = CreateRepoT([], lookups);
            var item = new LookupItem { Name = "Test", Active = true };

            // Act
            await repo.InsertLookupItemAsync(lookupId, item);

            // Assert
            Assert.NotNull(item);
            Assert.Equal("Test", item.Name);
            Assert.True(item.Active);
        }

        [Fact]
        public async Task DeleteLookupItemAsync_ThrowsArgumentException_WhenDeleteCommandMissing()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup>
    {
        new() { Id = lookupId, DeleteCommand = null! }
    };
            var repo = CreateRepoT([], lookups);
            var item = new LookupItem { Id = Guid.NewGuid(), Active = true };
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteLookupItemAsync(lookupId, item));
        }

        [Fact]
        public async Task DeleteLookupItemAsync_Executes_WhenAllowed()
        {
            var lookupId = Guid.NewGuid();
            var lookups = new List<Lookup>
            {
                new() { Id = lookupId, DeleteCommand = "spDelete" }
            };
            var repo = CreateRepoT([], lookups);
            var item = new LookupItem { Id = Guid.NewGuid(), Active = true };

            // Act
            await repo.DeleteLookupItemAsync(lookupId, item);

            // Assert
            Assert.NotNull(item);
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.True(item.Active);
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_ReturnsFilteredItems()
        {
            // Arrange
            var virusFamilyId = Guid.NewGuid();
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Type1", Parent = virusFamilyId, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Type2", Parent = virusFamilyId, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Type3", Parent = Guid.NewGuid(), Active = true }
            };

            bool called = false;
            var repo = CreateRepoWithByParent(
                items,
                (lookup, parent) => {
                    called = true;
                    Assert.Equal("VirusType", lookup);
                    Assert.Equal(virusFamilyId, parent);
                    return Task.FromResult(items.Where(i => i.Parent == parent).AsEnumerable());
                }
            );

            // Act
            var result = await repo.GetAllVirusTypesByParentAsync(virusFamilyId);

            // Assert
            Assert.True(called);
            Assert.Equal(2, result.Count());
            Assert.All(result, x => Assert.Equal(virusFamilyId, x.Parent));
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_ReturnsFilteredItems()
        {
            // Arrange
            var hostSpeciesId = Guid.NewGuid();
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Breed1", Parent = hostSpeciesId, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Breed2", Parent = hostSpeciesId, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Breed3", Parent = Guid.NewGuid(), Active = true }
            };

            bool called = false;
            var repo = CreateRepoWithByParent(
                items,
                (lookup, parent) => {
                    called = true;
                    Assert.Equal("HostBreed", lookup);
                    Assert.Equal(hostSpeciesId, parent);
                    return Task.FromResult(items.Where(i => i.Parent == parent).AsEnumerable());
                }
            );

            // Act
            var result = await repo.GetAllHostBreedsByParentAsync(hostSpeciesId);

            // Assert
            Assert.True(called);
            Assert.Equal(2, result.Count());
            Assert.All(result, x => Assert.Equal(hostSpeciesId, x.Parent));
        }

        [Fact]
        public async Task GetAllTraysByParentAsync_ReturnsFilteredItems()
        {
            // Arrange
            var freezerId = Guid.NewGuid();
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Tray1", Parent = freezerId, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Tray2", Parent = freezerId, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Tray3", Parent = Guid.NewGuid(), Active = true }
            };

            bool called = false;
            var repo = CreateRepoWithByParent(
                items,
                (lookup, parent) => {
                    called = true;
                    Assert.Equal("Tray", lookup);
                    Assert.Equal(freezerId, parent);
                    return Task.FromResult(items.Where(i => i.Parent == parent).AsEnumerable());
                }
            );

            // Act
            var result = await repo.GetAllTraysByParentAsync(freezerId);

            // Assert
            Assert.True(called);
            Assert.Equal(2, result.Count());
            Assert.All(result, x => Assert.Equal(freezerId, x.Parent));
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_ReturnsFilteredActiveItems()
        {
            // Arrange
            var virusFamilyId = Guid.NewGuid();
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Type1", Parent = virusFamilyId, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Type2", Parent = virusFamilyId, Active = false }, // inactive
                new() { Id = Guid.NewGuid(), Name = "Type3", Parent = Guid.NewGuid(), Active = true }  // different parent
            };

            var repo = CreateRepoWithByParent(
                items,
                (lookup, parent) => {
                    // Only return active items matching the parent
                    var filteredItems = items
                        .Where(i => i.Parent == parent && i.Active)
                        .ToList();
                    return Task.FromResult(filteredItems.AsEnumerable());
                }
            );

            // Act
            var result = await repo.GetAllVirusTypesByParentAsync(virusFamilyId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Type1", result.First().Name);
            Assert.Equal(virusFamilyId, result.First().Parent);
            Assert.True(result.First().Active);
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_ReturnsEmptyList_WhenNoMatches()
        {
            // Arrange
            var nonExistentParentId = Guid.NewGuid();
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Breed1", Parent = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Name = "Breed2", Parent = Guid.NewGuid(), Active = true }
            };

            var repo = CreateRepoWithByParent(
                items,
                (lookup, parent) => {
                    return Task.FromResult(items.Where(i => i.Parent == parent && i.Active).AsEnumerable());
                }
            );

            // Act
            var result = await repo.GetAllHostBreedsByParentAsync(nonExistentParentId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllTraysByParentAsync_ReturnsOnlyActiveItems()
        {
            // Arrange
            var freezerId = Guid.NewGuid();
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Name = "Tray1", Parent = freezerId, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Tray2", Parent = freezerId, Active = false },
                new() { Id = Guid.NewGuid(), Name = "Tray3", Parent = freezerId, Active = true }
            };

            var repo = CreateRepoWithByParent(
                items,
                (lookup, parent) => {
                    return Task.FromResult(items.Where(i => i.Parent == parent && i.Active).AsEnumerable());
                }
            );

            // Act
            var result = await repo.GetAllTraysByParentAsync(freezerId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, x => Assert.True(x.Active));
            Assert.Contains(result, x => x.Name == "Tray1");
            Assert.Contains(result, x => x.Name == "Tray3");
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_HandlesNullParent()
        {
            // Arrange
            Guid? nullParent = null;
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Name = "TypeNull1", Parent = null, Active = true },
                new() { Id = Guid.NewGuid(), Name = "TypeNull2", Parent = null, Active = true },
                new() { Id = Guid.NewGuid(), Name = "Type3", Parent = Guid.NewGuid(), Active = true }
            };

            bool called = false;
            var repo = CreateRepoWithByParent(
                items,
                (lookup, parent) => {
                    called = true;
                    Assert.Equal("VirusType", lookup);
                    Assert.Null(parent);

                    // Filter for items with null parent
                    return Task.FromResult(items.Where(i => i.Parent == parent && i.Active).AsEnumerable());
                }
            );

            // Act
            var result = await repo.GetAllVirusTypesByParentAsync(nullParent);

            // Assert
            Assert.True(called);
            Assert.Equal(2, result.Count());
            Assert.All(result, x => Assert.Null(x.Parent));
        }
    }
}
