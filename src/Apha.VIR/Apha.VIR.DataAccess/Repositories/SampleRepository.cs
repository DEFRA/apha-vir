using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class SampleRepository : ISampleRepository
{
    private readonly VIRDbContext _context;
    public SampleRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Sample>> GetSamplesBySubmissionIdAsync(Guid submissionId)
    {
        return await _context.Set<Sample>()
                   .FromSqlInterpolated($"EXEC spSampleGetBySubmission @SubmissionId = {submissionId}")
                   .ToListAsync();
    }
}
