using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
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

    public async Task AddSampleAsync(Sample sample, string avNumber, string User)
    {        
        if (!string.IsNullOrEmpty(avNumber))
        {
            var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Avnumber == avNumber);
            sample.SampleSubmissionId = submission?.SubmissionId ?? Guid.Empty;
        }

        sample.SampleNumber = _context.Samples.Select(e => e.SampleNumber).OrderByDescending(n => n).FirstOrDefault() + 1;        

        var parameters = new[]
        {
           new SqlParameter("@UserId", SqlDbType.VarChar, 20) { Value = User },
           new SqlParameter("@sampleID", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() },
           new SqlParameter("@SampleSubmissionId", SqlDbType.UniqueIdentifier) { Value = (object?)sample.SampleSubmissionId ?? Guid.Empty },
           new SqlParameter("@SampleNumber", SqlDbType.Int) { Value = sample.SampleNumber },
           new SqlParameter("@SMSReferenceNumber", SqlDbType.VarChar, 30) { Value = (object?)sample.SmsreferenceNumber ?? DBNull.Value },
           new SqlParameter("@SenderReferenceNumber", SqlDbType.VarChar, 50) { Value = (object?)sample.SenderReferenceNumber  ?? DBNull.Value },
           new SqlParameter("@SampleType", SqlDbType.UniqueIdentifier) { Value = (object?)sample.SampleType ?? Guid.Empty },
           new SqlParameter("@HostSpecies", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostSpecies ?? Guid.Empty },
           new SqlParameter("@HostBreed", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostBreed ?? Guid.Empty },
           new SqlParameter("@HostPurpose", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostPurpose ?? Guid.Empty },
           new SqlParameter("@SamplingLocationHouse",  SqlDbType.VarChar, 50) { Value = (object?)sample.SamplingLocationHouse ?? DBNull.Value },          
           new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = (object?)sample.LastModified ?? DBNull.Value, Direction = ParameterDirection.InputOutput }
        };

        await _context.Database.ExecuteSqlRawAsync(
           @"EXEC spSampleInsert @UserID, @sampleID, @SampleSubmissionId, @SampleNumber, @SMSReferenceNumber, 
                @SenderReferenceNumber, @SampleType, @HostSpecies, @HostBreed, @HostPurpose, @SamplingLocationHouse, @LastModified OUTPUT",
           parameters);
    }

    public async Task UpdateSampleAsync(Sample sample, string User)
    {
        await _context.Database.ExecuteSqlRawAsync(
           "EXEC spSampleUpdate @UserID, @sampleID, @SampleSubmissionId, @SampleNumber, @SMSReferenceNumber, @SenderReferenceNumber, @SampleType, @HostSpecies, @HostBreed, @HostPurpose, @SamplingLocationHouse, @LastModified OUTPUT",
                new SqlParameter("@UserId", SqlDbType.VarChar, 20) { Value = User },
                new SqlParameter("@sampleID", SqlDbType.UniqueIdentifier) { Value = (object?)sample.SampleId ?? Guid.Empty },
                new SqlParameter("@SampleSubmissionId", SqlDbType.UniqueIdentifier) { Value = (object?)sample.SampleSubmissionId ?? Guid.Empty },
                new SqlParameter("@SampleNumber", SqlDbType.Int) { Value = sample.SampleNumber },
                new SqlParameter("@SMSReferenceNumber", SqlDbType.VarChar, 30) { Value = (object?)sample.SmsreferenceNumber ?? DBNull.Value },
                new SqlParameter("@SenderReferenceNumber", SqlDbType.VarChar, 50) { Value = (object?)sample.SenderReferenceNumber ?? DBNull.Value },
                new SqlParameter("@SampleType", SqlDbType.UniqueIdentifier) { Value = (object?)sample.SampleType ?? Guid.Empty },
                new SqlParameter("@HostSpecies", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostSpecies ?? Guid.Empty },
                new SqlParameter("@HostBreed", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostBreed ?? Guid.Empty },
                new SqlParameter("@HostPurpose", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostPurpose ?? Guid.Empty },
                new SqlParameter("@SamplingLocationHouse", SqlDbType.VarChar, 50) { Value = (object?)sample.SamplingLocationHouse ?? DBNull.Value },
                new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = (object?)sample.LastModified ?? DBNull.Value, Direction = ParameterDirection.InputOutput }
           );
    }

}
