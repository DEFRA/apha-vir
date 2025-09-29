using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class SubmissionRepository : RepositoryBase<Submission>, ISubmissionRepository
{
    public SubmissionRepository(VIRDbContext context) : base(context)
    {
    }
    public async Task<bool> AVNumberExistsInVirAsync(string avNumber)
    {
        var countResult = await SqlQueryInterpolatedFor<int>($"EXEC spSubmissionCountByAVNumber @AVNumber = {avNumber}")
                .ToListAsync();

        var count = countResult.FirstOrDefault();

        return count > 0;
    }

    public virtual async Task<Submission> GetSubmissionDetailsByAVNumberAsync(string avNumber)
    {
        Submission submission = null!;
        using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "spSubmissionGetByAVNumber";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@AVNumber";
                param.Value = avNumber;
                command.Parameters.Add(param);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    submission = await GetSubmissionDetail(reader);
                }
            }
        }
        return submission;
    }

    protected virtual async Task<Submission> GetSubmissionDetail(SqlDataReader reader)
    {
        Submission submission = null!;
        if (await reader.ReadAsync())
        {
            submission = new Submission
            {
                SubmissionId = (Guid)reader["SubmissionId"],
                Avnumber = reader.GetString("AVNumber"),
                SendersReferenceNumber = SqlReaderHelper.GetNullableString(reader, "SendersReferenceNumber"),
                RlreferenceNumber = SqlReaderHelper.GetNullableString(reader, "RLReferenceNumber"),
                SubmittingLab = SqlReaderHelper.GetNullableGuid(reader, "SubmittingLab"),
                Sender = SqlReaderHelper.GetNullableString(reader, "Sender"),
                SenderOrganisation = SqlReaderHelper.GetNullableString(reader, "SenderOrganisation"),
                SenderAddress = SqlReaderHelper.GetNullableString(reader, "SenderAddress"),
                CountryOfOrigin = SqlReaderHelper.GetNullableGuid(reader, "CountryOfOrigin"),
                SubmittingCountry = SqlReaderHelper.GetNullableGuid(reader, "SubmittingCountry"),
                ReasonForSubmission = SqlReaderHelper.GetNullableGuid(reader, "ReasonForSubmission"),
                DateSubmissionReceived = SqlReaderHelper.GetNullableDateTime(reader, "DateSubmissionReceived"),
                Cphnumber = SqlReaderHelper.GetNullableString(reader, "CPHNumber"),
                Owner = SqlReaderHelper.GetNullableString(reader, "Owner"),
                SamplingLocationPremises = SqlReaderHelper.GetNullableString(reader, "SamplingLocationPremises"),
                NumberOfSamples = SqlReaderHelper.GetNullableInt(reader, "NumberOfSamples"),
                LastModified = (byte[])reader["LastModified"],
                CountryOfOriginName = SqlReaderHelper.GetNullableString(reader, "CountryOfOriginName"),
                SubmittingCountryName = SqlReaderHelper.GetNullableString(reader, "SubmittingCountryName")
            };
        }
        return submission;
    }

    public async Task AddSubmissionAsync(Submission submission, string user)
    {
        var parameters = new[]
        {
            new SqlParameter("@UserID", SqlDbType.VarChar, 120) { Value = user },
            new SqlParameter("@SubmissionId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() },
            new SqlParameter("@AVNumber", SqlDbType.VarChar, 20) { Value = submission.Avnumber },
            new SqlParameter("@SendersReferenceNumber", SqlDbType.VarChar, 50) { Value = (object?)submission.SendersReferenceNumber ?? DBNull.Value },
            new SqlParameter("@RLReferenceNumber", SqlDbType.VarChar, 20) { Value = (object?)submission.RlreferenceNumber ?? DBNull.Value },
            new SqlParameter("@SubmittingLab", SqlDbType.UniqueIdentifier) { Value = submission.SubmittingLab.HasValue ? submission.SubmittingLab : DBNull.Value },
            new SqlParameter("@Sender", SqlDbType.VarChar, 50) { Value = (object?)submission.Sender ?? DBNull.Value },
            new SqlParameter("@SenderOrganisation", SqlDbType.VarChar, 200) { Value = (object?)submission.SenderOrganisation ?? DBNull.Value },
            new SqlParameter("@SenderAddress", SqlDbType.VarChar, 500) { Value = (object?)submission.SenderAddress ?? DBNull.Value },
            new SqlParameter("@CountryOfOrigin", SqlDbType.UniqueIdentifier) { Value = submission.CountryOfOrigin.HasValue ? submission.CountryOfOrigin : DBNull.Value },
            new SqlParameter("@SubmittingCountry", SqlDbType.UniqueIdentifier) { Value = submission.SubmittingCountry.HasValue ? submission.SubmittingCountry : DBNull.Value },
            new SqlParameter("@ReasonForSubmission", SqlDbType.UniqueIdentifier) { Value = submission.ReasonForSubmission.HasValue ? submission.ReasonForSubmission : DBNull.Value },
            new SqlParameter("@DateSubmissionReceived", SqlDbType.DateTime) { Value = (object?)submission.DateSubmissionReceived ?? DBNull.Value },
            new SqlParameter("@CPHNumber", SqlDbType.VarChar, 14) { Value = (object?)submission.Cphnumber ?? DBNull.Value },
            new SqlParameter("@Owner", SqlDbType.VarChar, 50) { Value = (object?)submission.Owner ?? DBNull.Value },
            new SqlParameter("@SamplingLocationPremises", SqlDbType.VarChar, 500) { Value = (object?)submission.SamplingLocationPremises ?? DBNull.Value },
            new SqlParameter("@NumberOfSamples", SqlDbType.Int) { Value = (object?)submission.NumberOfSamples ?? DBNull.Value },
            new SqlParameter("@LastModified", SqlDbType.Timestamp) { Direction = ParameterDirection.Output }
        };

        await ExecuteSqlAsync(
          @"EXEC spSubmissionInsert @UserID, @SubmissionId, @AVNumber, @SendersReferenceNumber, @RLReferenceNumber, @SubmittingLab,  
            @Sender, @SenderOrganisation, @SenderAddress, @CountryOfOrigin, @SubmittingCountry, @ReasonForSubmission, 
            @DateSubmissionReceived, @CPHNumber, @Owner, @SamplingLocationPremises, @NumberOfSamples, @LastModified OUTPUT",
          parameters);
    }

    public async Task UpdateSubmissionAsync(Submission submission, string user)
    {
        var parameters = new[]
        {
            new SqlParameter("@UserID", SqlDbType.VarChar, 120) { Value = user },
            new SqlParameter("@SubmissionId", SqlDbType.UniqueIdentifier) { Value = submission.SubmissionId },
            new SqlParameter("@AVNumber", SqlDbType.VarChar, 20) { Value = submission.Avnumber },
            new SqlParameter("@SendersReferenceNumber", SqlDbType.VarChar, 50) { Value = (object?)submission.SendersReferenceNumber ?? DBNull.Value },
            new SqlParameter("@RLReferenceNumber", SqlDbType.VarChar, 20) { Value = (object?)submission.RlreferenceNumber ?? DBNull.Value },
            new SqlParameter("@SubmittingLab", SqlDbType.UniqueIdentifier) { Value = submission.SubmittingLab.HasValue ? submission.SubmittingLab : DBNull.Value },
            new SqlParameter("@Sender", SqlDbType.VarChar, 50) { Value = (object?)submission.Sender ?? DBNull.Value },
            new SqlParameter("@SenderOrganisation", SqlDbType.VarChar, 200) { Value = (object?)submission.SenderOrganisation ?? DBNull.Value },
            new SqlParameter("@SenderAddress", SqlDbType.VarChar, 500) { Value = (object?)submission.SenderAddress ?? DBNull.Value },
            new SqlParameter("@CountryOfOrigin", SqlDbType.UniqueIdentifier) { Value = submission.CountryOfOrigin.HasValue ? submission.CountryOfOrigin : DBNull.Value },
            new SqlParameter("@SubmittingCountry", SqlDbType.UniqueIdentifier) { Value = submission.SubmittingCountry.HasValue ? submission.SubmittingCountry : DBNull.Value },
            new SqlParameter("@ReasonForSubmission", SqlDbType.UniqueIdentifier) { Value = submission.ReasonForSubmission.HasValue ? submission.ReasonForSubmission : DBNull.Value },
            new SqlParameter("@DateSubmissionReceived", SqlDbType.DateTime) { Value = (object?)submission.DateSubmissionReceived ?? DBNull.Value },
            new SqlParameter("@CPHNumber", SqlDbType.VarChar, 14) { Value = (object?)submission.Cphnumber ?? DBNull.Value },
            new SqlParameter("@Owner", SqlDbType.VarChar, 50) { Value = (object?)submission.Owner ?? DBNull.Value },
            new SqlParameter("@SamplingLocationPremises", SqlDbType.VarChar, 500) { Value = (object?)submission.SamplingLocationPremises ?? DBNull.Value },
            new SqlParameter("@NumberOfSamples", SqlDbType.Int) { Value = (object?)submission.NumberOfSamples ?? DBNull.Value },
            new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = submission.LastModified }
        };

        await ExecuteSqlAsync(
          @"EXEC spSubmissionUpdate @UserID, @SubmissionId, @AVNumber, @SendersReferenceNumber, @RLReferenceNumber, @SubmittingLab, 
            @Sender, @SenderOrganisation, @SenderAddress, @CountryOfOrigin, @SubmittingCountry, @ReasonForSubmission, 
            @DateSubmissionReceived, @CPHNumber, @Owner, @SamplingLocationPremises, @NumberOfSamples, @LastModified OUTPUT",
          parameters);
    }

    public async Task DeleteSubmissionAsync(Guid submissionId, string userId, byte[] lastModified)
    {
        await ExecuteSqlAsync(
           "EXEC spSubmissionDelete @UserID, @SubmissionId, @LastModified",
           new SqlParameter("@UserID", SqlDbType.VarChar, 120) { Value = userId },
           new SqlParameter("@SubmissionId", SqlDbType.UniqueIdentifier) { Value = submissionId },
           new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = lastModified }
        );
    }

    public async Task<IEnumerable<string>> GetLatestSubmissionsAsync()
    {
        return await SqlQueryInterpolatedFor<string>($"EXEC spLastAVNumbersModified").ToListAsync();
    }
}
