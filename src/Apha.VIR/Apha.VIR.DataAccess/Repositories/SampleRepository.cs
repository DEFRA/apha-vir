using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class SampleRepository : RepositoryBase<Sample>,ISampleRepository
{
 
    public SampleRepository(VIRDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<Sample>> GetSamplesBySubmissionIdAsync(Guid submissionId)
    {
        return await _context.Set<Sample>()
                   .FromSqlInterpolated($"EXEC spSampleGetBySubmission @SubmissionId = {submissionId}")
                   .ToListAsync();
    }

    public async Task<Sample?> GetSampleAsync(string avNumber, Guid? sampleId)
    {
        if (string.IsNullOrEmpty(avNumber) || !sampleId.HasValue)
            return null;

        var submissionId = await GetDbSetFor<Submission>()
            .Where(s => s.Avnumber == avNumber).Select(s => s.SubmissionId).FirstOrDefaultAsync();

        if (submissionId == Guid.Empty)
            return null;

        return await _context.Samples
            .FirstOrDefaultAsync(s => s.SampleSubmissionId == submissionId && s.SampleId == sampleId.Value);
    }

    public async Task AddSampleAsync(Sample sample, string avNumber, string User)
    {
        if (!string.IsNullOrEmpty(avNumber))
        {
            var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Avnumber == avNumber);
            if (submission != null)
                sample.SampleSubmissionId = submission.SubmissionId;
        }

        sample.SampleNumber = await _context.Samples.Select(e => e.SampleNumber).OrderByDescending(n => n).FirstOrDefaultAsync() + 1;

        var parameters = new[]
        {
           new SqlParameter("@UserId", SqlDbType.VarChar, 20) { Value = User },
           new SqlParameter("@sampleID", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() },
           new SqlParameter("@SampleSubmissionId", SqlDbType.UniqueIdentifier) { Value = sample.SampleSubmissionId },
           new SqlParameter("@SampleNumber", SqlDbType.Int) { Value = sample.SampleNumber },
           new SqlParameter("@SMSReferenceNumber", SqlDbType.VarChar, 30) { Value = (object?)sample.SMSReferenceNumber ?? DBNull.Value },
           new SqlParameter("@SenderReferenceNumber", SqlDbType.VarChar, 50) { Value = (object?)sample.SenderReferenceNumber  ?? DBNull.Value },
           new SqlParameter("@SampleType", SqlDbType.UniqueIdentifier) { Value = (object?)sample.SampleType ?? DBNull.Value },
           new SqlParameter("@HostSpecies", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostSpecies ?? DBNull.Value },
           new SqlParameter("@HostBreed", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostBreed ?? DBNull.Value },
           new SqlParameter("@HostPurpose", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostPurpose ?? DBNull.Value },
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
                new SqlParameter("@sampleID", SqlDbType.UniqueIdentifier) { Value = sample.SampleId },
                new SqlParameter("@SampleSubmissionId", SqlDbType.UniqueIdentifier) { Value = sample.SampleSubmissionId },
                new SqlParameter("@SampleNumber", SqlDbType.Int) { Value = sample.SampleNumber },
                new SqlParameter("@SMSReferenceNumber", SqlDbType.VarChar, 30) { Value = (object?)sample.SMSReferenceNumber ?? DBNull.Value },
                new SqlParameter("@SenderReferenceNumber", SqlDbType.VarChar, 50) { Value = (object?)sample.SenderReferenceNumber ?? DBNull.Value },
                new SqlParameter("@SampleType", SqlDbType.UniqueIdentifier) { Value = (object?)sample.SampleType ?? DBNull.Value },
                new SqlParameter("@HostSpecies", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostSpecies ?? DBNull.Value },
                new SqlParameter("@HostBreed", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostBreed ?? DBNull.Value },
                new SqlParameter("@HostPurpose", SqlDbType.UniqueIdentifier) { Value = (object?)sample.HostPurpose ?? DBNull.Value },
                new SqlParameter("@SamplingLocationHouse", SqlDbType.VarChar, 50) { Value = (object?)sample.SamplingLocationHouse ?? DBNull.Value },
                new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = (object?)sample.LastModified ?? DBNull.Value, Direction = ParameterDirection.InputOutput }
           );
    }

    public async Task DeleteSampleAsync(Guid sampleId, string userId, byte[] lastModified)
    {
        await _context.Database.ExecuteSqlRawAsync(
           "EXEC spSampleDelete @UserID, @SampleId, @LastModified",
           new SqlParameter("@UserID", SqlDbType.UniqueIdentifier) { Value = userId },
           new SqlParameter("@SampleId", SqlDbType.VarChar, 20) { Value = sampleId },           
           new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = lastModified }
        );
    }

}