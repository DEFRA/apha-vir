using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;

namespace Apha.VIR.DataAccess.UnitTests.Repository.VirusCharacteristicAssociationRepositoryTest
{
    public class TestVirusCharacteristicAssociationRepository : VirusCharacteristicAssociationRepository
    {
        public bool AssignCalled { get; private set; }
        public bool RemoveCalled { get; private set; }

        public TestVirusCharacteristicAssociationRepository(VIRDbContext context) : base(context) { }

        protected override Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql)
        {
            var query = sql.Format.ToLowerInvariant();
            var args = sql.GetArguments().Select(a => a?.ToString()?.ToLowerInvariant()).ToArray();

            if (query.Contains("spviruscharacteristiclinkupdate"))
            {
                if (args.Any(a => a == "assign"))
                {
                    AssignCalled = true;
                    return Task.FromResult(1);
                }
                if (args.Any(a => a == "remove"))
                {
                    RemoveCalled = true;
                    return Task.FromResult(1);
                }
            }

            throw new NotImplementedException($"No override for query {sql.Format}");
        }

    }

    public class VirusCharacteristicAssociationRepositoryTests
    {
        [Fact]
        public async Task AssignCharacteristicToTypeAsync_CallsExecuteSqlInterpolatedAsync_WithAssignMode()
        {
            var virusTypeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();

            var dbContext = new VIRDbContext(); // Replace null with a valid VIRDbContext instance
            var repo = new TestVirusCharacteristicAssociationRepository(dbContext);

            await repo.AssignCharacteristicToTypeAsync(virusTypeId, characteristicId);

            Assert.True(repo.AssignCalled);
        }

        [Fact]
        public async Task RemoveCharacteristicFromTypeAsync_CallsExecuteSqlInterpolatedAsync_WithRemoveMode()
        {
            var virusTypeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();

            var dbContext = new VIRDbContext(); // Replace null with a valid VIRDbContext instance
            var repo = new TestVirusCharacteristicAssociationRepository(dbContext);

            await repo.RemoveCharacteristicFromTypeAsync(virusTypeId, characteristicId);

            Assert.True(repo.RemoveCalled);
        }
    }
}
