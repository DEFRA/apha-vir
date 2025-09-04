using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateRepository : IIsolateRepository
{
    private readonly VIRDbContext _context;
    private const string NoOfAliquots = "NoOfAliquots";
    public IsolateRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<IsolateInfo>> GetIsolateInfoByAVNumberAsync(string AVNumber)
    {
        var isolateInfoList = new List<IsolateInfo>();

        using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "spIsolateGetByAVNumber";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@AVNumber";
                param.Value = AVNumber;
                command.Parameters.Add(param);

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var dto = new IsolateInfo
                        {
                            AvNumber = result["AVNumber"] as string,
                            Nomenclature = result["Nomenclature"] as string,
                            IsolateId = (Guid)result["IsolateId"],
                            NoOfAliquots = (int)result[NoOfAliquots],
                            ValidToIssue = (result["ValidToIssue"] != DBNull.Value ? (bool?)result["ValidToIssue"] : false),
                            IsMixedIsolate = (bool)result["IsMixedIsolate"],
                            MaterialTransferAgreement = (bool)result["MaterialTransferAgreement"],
                            IsolateSampleId = (Guid)result["IsolateSampleId"],
                            YearOfIsolation = (result["YearOfIsolation"] != DBNull.Value ? (int?)result["YearOfIsolation"] : null),
                        };
                        isolateInfoList.Add(dto);
                    }
                }
            }
        }

        return isolateInfoList;
    }

    public async Task<IsolateFullDetail> GetIsolateFullDetailsByIdAsync(Guid isolateId)
    {
        IsolateFullDetail isolateFullDetail = new IsolateFullDetail
        {
            IsolateCharacteristicDetails = new(),
            IsolateDetails = new(),
            IsolateDispatchDetails = new(),
            IsolateViabilityDetails = new(),
        };

        using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "spIsolateGetFullDetails";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@IsolateID";
                param.Value = isolateId;
                command.Parameters.Add(param);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    isolateFullDetail.IsolateDetails = await GetIsolateDetail(reader);

                    await reader.NextResultAsync();

                    isolateFullDetail.IsolateDispatchDetails = (await GetIsolateDispatchDetail(reader, isolateId)).ToList()!;

                    await reader.NextResultAsync();

                    isolateFullDetail.IsolateViabilityDetails = (await GetIsolateViabilityDetail(reader, isolateId)).ToList()!;

                    await reader.NextResultAsync();

                    isolateFullDetail.IsolateCharacteristicDetails = (await GetIsolateCharacteristicDetail(reader, isolateId)).ToList()!;
                }
            }
        }

        return isolateFullDetail;
    }

    public async Task<Isolate> GetIsolateByIsolateAndAVNumberAsync(string avNumber, Guid isolateId)
    {
        var parameters = new[]
        {
            new SqlParameter("@AVNumber", avNumber),
        };

        List<Isolate> isolateList = await _context.Set<Isolate>()
           .FromSqlRaw($"EXEC spIsolateGetByAVNumber  @AVNumber ", parameters).ToListAsync();

        Isolate isolate = isolateList.First(i => i.IsolateId == isolateId);
        return isolate;
    }

    public async Task<Guid> AddIsolateDetailsAsync(Isolate isolate)
    {
        isolate.IsolateId = Guid.NewGuid();
        int nextIsolateNumber = await _context.Isolates
        .Select(u => u.IsolateNumber)
        .MaxAsync() ?? 0;
        isolate.IsolateNumber = nextIsolateNumber + 1;

        SqlParameter[] parameters = GetSqlParameters(isolate);
        parameters = parameters.Concat(new[] { new SqlParameter("@LastModified", SqlDbType.Timestamp) { Direction = ParameterDirection.Output } })
        .ToArray();

        await _context.Database.ExecuteSqlRawAsync(
          @"EXEC spIsolateInsert @UserID, @IsolateId, @IsolateSampleId, @IsolateNumber, @Family, @Type, @YearOfIsolation,
            @IsMixedIsolate, @IsolationMethod, @AntiserumProduced, @AntigenProduced, @PhylogeneticAnalysis, @MaterialTransferAgreement,  
            @MTALocation, @Comment, @ValidToIssue, @WhyNotValidToIssue, @OriginalSampleAvailable, @FirstViablePassageNumber,
            @NoOfAliquots, @Freezer, @Tray, @Well, @IsolateNomenclature, @SMSReferenceNumber, @PhylogeneticFileName, @LastModified OUTPUT",
          parameters);

        await AddIsolateCharacteristicsAsync(isolate.IsolateId, isolate.Type, isolate.CreatedBy!);

        return isolate.IsolateId;
    }

    public async Task UpdateIsolateDetailsAsync(Isolate isolate)
    {
        SqlParameter[] parameters = GetSqlParameters(isolate);
        parameters = parameters.Concat(new[] { new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = isolate.LastModified } })
        .ToArray();

        await _context.Database.ExecuteSqlRawAsync(
          @"EXEC spIsolateUpdate @UserID, @IsolateId, @IsolateSampleId, @IsolateNumber, @Family, @Type, @YearOfIsolation,
            @IsMixedIsolate, @IsolationMethod, @AntiserumProduced, @AntigenProduced, @PhylogeneticAnalysis, @MaterialTransferAgreement,  
            @MTALocation, @Comment, @ValidToIssue, @WhyNotValidToIssue, @OriginalSampleAvailable, @FirstViablePassageNumber,
            @NoOfAliquots, @Freezer, @Tray, @Well, @IsolateNomenclature, @SMSReferenceNumber, @PhylogeneticFileName, @LastModified OUTPUT",
          parameters);
    }

    private async Task AddIsolateCharacteristicsAsync(Guid isolateId, Guid type, string User)
    {
        var parameters = new[]
        {
            new SqlParameter("@UserID", SqlDbType.VarChar, 20) { Value = User },
            new SqlParameter("@IsolateId", SqlDbType.UniqueIdentifier) { Value = isolateId },
            new SqlParameter("@Type", SqlDbType.UniqueIdentifier) { Value = type }
        };

        await _context.Database.ExecuteSqlRawAsync(
          @"EXEC spIsolateCharacteristicsInsert @IsolateId, @Type, @UserID",
          parameters);
    }

    private static async Task<IsolateInfo> GetIsolateDetail(SqlDataReader reader)
    {
        IsolateInfo dto = null!;

        while (await reader.ReadAsync())
        {
            dto = new IsolateInfo
            {
                AvNumber = reader["AVNumber"].ToString(),
                FamilyName = reader["FamilyName"].ToString(),
                TypeName = reader["TypeName"].ToString(),
                GroupSpeciesName = reader["GroupSpeciesName"].ToString(),
                BreedName = reader["BreedName"].ToString(),
                CountryOfOriginName = reader["CountryOfOriginName"].ToString(),
                YearOfIsolation = reader["YearOfIsolation"] as int?,
                ReceivedDate = reader["ReceivedDate"] as DateTime?,
                FreezerName = reader["FreezerName"].ToString(),
                TrayName = reader["TrayName"].ToString(),
                Well = reader["Well"].ToString(),
                MaterialTransferAgreement = Convert.ToBoolean(reader["MaterialTransferAgreement"]),
                NoOfAliquots = (reader[NoOfAliquots] != DBNull.Value ? Convert.ToInt32(reader[NoOfAliquots]) : 0),
                IsolateId = (Guid)reader["IsolateID"],
                SenderReferenceNumber = reader["SenderReferenceNumber"].ToString(),
                IsolationMethodName = reader["IsolationMethodName"].ToString(),
                AntiserumProduced = Convert.ToBoolean(reader["AntiserumProduced"]),
                AntigenProduced = Convert.ToBoolean(reader["AntigenProduced"]),
                PhylogeneticAnalysis = reader["PhylogeneticAnalysis"].ToString(),
                PhylogeneticFileName = reader["PhylogeneticFileName"].ToString(),
                Mtalocation = reader["MTALocation"].ToString(),
                Comment = reader["Comment"].ToString(),
                ValidToIssue = reader["ValidToIssue"] as bool?,
                WhyNotValidToIssue = reader["WhyNotValidToIssue"].ToString(),
                OriginalSampleAvailable = Convert.ToBoolean(reader["OriginalSampleAvailable"]),
                FirstViablePassageNumber = reader["FirstViablePassageNumber"] as int?,
                IsMixedIsolate = Convert.ToBoolean(reader["IsMixedIsolate"]),
                Nomenclature = reader["Nomenclature"].ToString(),
                SmsreferenceNumber = reader["SMSReferenceNumber"].ToString(),
                HostPurposeName = reader["HostPurposeName"].ToString(),
                SampleTypeName = reader["SampleTypeName"].ToString()
            };
        }

        return dto;
    }

    private static async Task<IEnumerable<IsolateDispatchInfo>> GetIsolateDispatchDetail(SqlDataReader reader, Guid isolateId)
    {
        List<IsolateDispatchInfo> dispatchInfos = [];

        while (await reader.ReadAsync())
        {
            dispatchInfos.Add(new IsolateDispatchInfo
            {
                NoOfAliquots = Convert.ToInt32(reader[NoOfAliquots]),
                PassageNumber = Convert.ToInt32(reader["PassageNumber"]),
                RecipientName = reader["RecipientName"].ToString(),
                RecipientAddress = reader["RecipientAddress"].ToString(),
                ReasonForDispatch = reader["ReasonForDispatch"].ToString(),
                DispatchedDate = Convert.ToDateTime(reader["DispatchedDate"]),
                DispatchedByName = reader["DispatchedByName"].ToString(),
                DispatchIsolateId = isolateId
            });
        }

        return dispatchInfos;
    }

    private static async Task<IEnumerable<IsolateViabilityInfo>> GetIsolateViabilityDetail(SqlDataReader reader, Guid isolateId)
    {
        List<IsolateViabilityInfo> viabilityInfos = [];

        while (await reader.ReadAsync())
        {
            viabilityInfos.Add(new IsolateViabilityInfo
            {
                ViabilityStatus = reader["ViabilityStatus"].ToString()!,
                DateChecked = Convert.ToDateTime(reader["DateChecked"]),
                CheckedByName = reader["CheckedByName"].ToString()!,
                IsolateViabilityIsolateId = isolateId
            });
        }
        return viabilityInfos;
    }

    private static async Task<IEnumerable<IsolateCharacteristicInfo>> GetIsolateCharacteristicDetail(SqlDataReader reader, Guid isolateId)
    {
        List<IsolateCharacteristicInfo> characteristicInfos = [];

        while (await reader.ReadAsync())
        {
            characteristicInfos.Add(new IsolateCharacteristicInfo
            {
                CharacteristicId = (Guid)reader["CharacteristicId"],
                CharacteristicName = reader["CharacteristicName"].ToString(),
                CharacteristicValue = reader["CharacteristicValue"].ToString(),
                CharacteristicPrefix = reader["CharacteristicPrefix"].ToString(),
                IsolateId = isolateId
            });
        }

        return characteristicInfos;
    }

    private static SqlParameter[] GetSqlParameters(Isolate isolate)
    {
        return new[]
        {
            new SqlParameter("@UserID", SqlDbType.VarChar, 20) { Value = isolate.CreatedBy },
            new SqlParameter("@IsolateId", SqlDbType.UniqueIdentifier) { Value = isolate.IsolateId },
            new SqlParameter("@IsolateSampleId", SqlDbType.UniqueIdentifier) { Value = isolate.IsolateSampleId },
            new SqlParameter("@IsolateNumber", SqlDbType.Int) { Value = (object?)isolate.IsolateNumber ?? DBNull.Value },
            new SqlParameter("@Family", SqlDbType.UniqueIdentifier) { Value = isolate.Family },
            new SqlParameter("@Type", SqlDbType.UniqueIdentifier) { Value = isolate.Type },
            new SqlParameter("@YearOfIsolation", SqlDbType.Int) { Value = (object?)isolate.YearOfIsolation ?? DBNull.Value },
            new SqlParameter("@IsMixedIsolate", SqlDbType.Bit) { Value = isolate.IsMixedIsolate },
            new SqlParameter("@IsolationMethod", SqlDbType.UniqueIdentifier) { Value = (object?)isolate.IsolationMethod ?? DBNull.Value },
            new SqlParameter("@AntiserumProduced", SqlDbType.Bit) { Value = isolate.AntiserumProduced },
            new SqlParameter("@AntigenProduced", SqlDbType.Bit) { Value =  isolate.AntigenProduced },
            new SqlParameter("@PhylogeneticAnalysis", SqlDbType.Text) { Value = (object?)isolate.PhylogeneticAnalysis ?? DBNull.Value },
            new SqlParameter("@MaterialtransferAgreement", SqlDbType.Bit) { Value = isolate.MaterialTransferAgreement },
            new SqlParameter("@MTALocation", SqlDbType.VarChar, 200) { Value = (object?)isolate.Mtalocation ?? DBNull.Value },
            new SqlParameter("@Comment", SqlDbType.Text) { Value = (object?)isolate.Comment ?? DBNull.Value },
            new SqlParameter("@ValidToIssue", SqlDbType.Bit) { Value = (object?)isolate.ValidToIssue ?? DBNull.Value },
            new SqlParameter("@WhyNotValidToIssue", SqlDbType.VarChar, 50) { Value = (object?)isolate.WhyNotValidToIssue ?? DBNull.Value },
            new SqlParameter("@OriginalSampleAvailable", SqlDbType.Bit) { Value = isolate.OriginalSampleAvailable },
            new SqlParameter("@FirstViablePassageNumber", SqlDbType.Int) { Value = isolate.FirstViablePassageNumber  },
            new SqlParameter("@NoOfAliquots", SqlDbType.Int) { Value = isolate.NoOfAliquots },
            new SqlParameter("@Freezer", SqlDbType.UniqueIdentifier) { Value = (object?)isolate.Freezer ?? DBNull.Value },
            new SqlParameter("@Tray", SqlDbType.UniqueIdentifier) { Value = (object?)isolate.Tray ?? DBNull.Value },
            new SqlParameter("@Well", SqlDbType.VarChar, 10) { Value = (object ?) isolate.Well ?? DBNull.Value },
            new SqlParameter("@IsolateNomenclature", SqlDbType.VarChar, 200) { Value = (object?)isolate.IsolateNomenclature ?? DBNull.Value },
            new SqlParameter("@SMSReferenceNumber", SqlDbType.VarChar, 30) { Value = (object?)isolate.SmsreferenceNumber ?? DBNull.Value },
            new SqlParameter("@PhylogeneticFileName", SqlDbType.VarChar, 100) { Value = (object?)isolate.PhylogeneticFileName ?? DBNull.Value }
        };
    }
}
