using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateRepository : IIsolateRepository
{
    private readonly VIRDbContext _context;

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
                            NoOfAliquots = result.GetInt32("NoOfAliquots"),
                            ValidToIssue = (result["ValidToIssue"] != DBNull.Value ? (bool?)result["ValidToIssue"] : false),
                            IsMixedIsolate = (bool)result["IsMixedIsolate"],
                            MaterialTransferAgreement = (bool)result["MaterialTransferAgreement"]
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
                NoOfAliquots = (reader["NoOfAliquots"] != DBNull.Value ? Convert.ToInt32(reader["NoOfAliquots"]) : 0),
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
                NoOfAliquots = Convert.ToInt32(reader["NoOfAliquots"]),
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
}
