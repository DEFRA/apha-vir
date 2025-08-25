using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Apha.VIR.DataAccess.Repositories;

public class SampleRepository : ISampleRepository
{
    private readonly VIRDbContext _context;

    public SampleRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Sample?> GetSampleAsync(string avNumber, Guid? sampleId)
    {
        if (string.IsNullOrEmpty(avNumber) || !sampleId.HasValue)
            return null;

        var submission = await _context.Submissions            
            .FirstOrDefaultAsync(s => s.Avnumber == avNumber);

        if (submission == null)
            return null;

        return await _context.Samples            
            .FirstOrDefaultAsync(s => s.SampleSubmissionId == submission.SubmissionId && s.SampleId == sampleId.Value);
    }
}
