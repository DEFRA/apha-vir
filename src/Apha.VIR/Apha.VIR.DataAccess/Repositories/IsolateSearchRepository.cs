using System.Collections.Generic;
using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Apha.VIR.DataAccess.Repositories
{
    public class IsolateSearchRepository : IIsolateSearchRepository
    {
        private readonly VIRDbContext _context;

        public IsolateSearchRepository(VIRDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedData<IsolateSearchResult>> PerformSearchAsync(PaginationParameters<SearchCriteria> criteria)
        {
            IQueryable<IsolateSearchResult> query = FetchIsolateSearchRecordsAsync(criteria);
            var totalRecords = await query.CountAsync();
            var isolateResult = await query.Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToListAsync();
            return new PagedData<IsolateSearchResult>(isolateResult, totalRecords);            
        }        

        private IQueryable<IsolateSearchResult> FetchIsolateSearchRecordsAsync(PaginationParameters<SearchCriteria> criteria)
        {
            var query = _context.VwIsolates.AsQueryable();

            if (!string.IsNullOrEmpty(criteria.Filter.AVNumber))
            {
                query = query.Where(i => i.Avnumber == criteria.Filter.AVNumber);
            }

            if (criteria.Filter.VirusFamily != Guid.Empty && criteria.Filter.VirusFamily != null)
            {
                query = query.Where(i => i.Family == criteria.Filter.VirusFamily);
            }

            if (criteria.Filter.VirusType != Guid.Empty && criteria.Filter.VirusType != null)
            {
                query = query.Where(i => i.Type == criteria.Filter.VirusType);
            }

            if (criteria.Filter.Group != Guid.Empty && criteria.Filter.Group != null)
            {
                query = query.Where(i => i.HostSpecies == criteria.Filter.Group);
            }

            if (criteria.Filter.Species != Guid.Empty && criteria.Filter.Species != null)
            {
                query = query.Where(i => i.HostBreed == criteria.Filter.Species);
            }

            if (criteria.Filter.CountryOfOrigin != Guid.Empty && criteria.Filter.CountryOfOrigin != null)
            {
                query = query.Where(i => i.CountryOfOrigin == criteria.Filter.CountryOfOrigin);
            }

            if (criteria.Filter.HostPurpose != Guid.Empty && criteria.Filter.HostPurpose != null)
            {
                query = query.Where(i => i.HostPurpose == criteria.Filter.HostPurpose);
            }

            if (criteria.Filter.SampleType != Guid.Empty && criteria.Filter.SampleType != null)
            {
                query = query.Where(i => i.SampleType == criteria.Filter.SampleType);
            }

            if (criteria.Filter.YearOfIsolation != 0)
            {
                query = query.Where(i => i.YearOfIsolation == criteria.Filter.YearOfIsolation);
            }

            if (criteria.Filter.ReceivedFromDate.HasValue && criteria.Filter.ReceivedToDate.HasValue)
            {
                query = query.Where(i => i.ReceivedDate >= criteria.Filter.ReceivedFromDate && i.ReceivedDate <= criteria.Filter.ReceivedToDate);
            }

            if (criteria.Filter.CreatedFromDate.HasValue && criteria.Filter.CreatedToDate.HasValue)
            {
                query = query.Where(i => i.DateCreated >= criteria.Filter.CreatedFromDate && i.DateCreated <= criteria.Filter.CreatedToDate);
            }

            foreach (CharacteristicCriteria characteristicItem in criteria.Filter.CharacteristicSearch)
            {
                if (characteristicItem.Characteristic != Guid.Empty && characteristicItem.Characteristic != null)
                {
                    query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                        c.CharacteristicIsolateId == i.IsolateId && c.VirusCharacteristicId == characteristicItem.Characteristic));

                    switch (characteristicItem.CharacteristicType)
                    {
                        case "Numeric":
                            switch (characteristicItem.Comparator)
                            {
                                case "between":
                                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                                    {
                                        query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                            float.Parse(c.CharacteristicValue) >= float.Parse(characteristicItem.CharacteristicValue1)));
                                    }
                                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue2))
                                    {
                                        query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                            float.Parse(c.CharacteristicValue) <= float.Parse(characteristicItem.CharacteristicValue2)));
                                    }
                                    break;
                                default:
                                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                                    {
                                        query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                            EF.Functions.Like(c.CharacteristicValue, $"{characteristicItem.Comparator}{characteristicItem.CharacteristicValue1}")));
                                    }
                                    break;
                            }
                            break;

                        case "SingleList":
                            switch (characteristicItem.Comparator)
                            {
                                case "begins with":
                                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                                    {
                                        query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                            EF.Functions.Like(c.CharacteristicValue, $"{characteristicItem.CharacteristicValue1}%")));
                                    }
                                    break;
                                case "not equal to":
                                    query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                        c.CharacteristicValue != characteristicItem.CharacteristicValue1));
                                    break;
                                default:
                                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                                    {
                                        query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                            c.CharacteristicValue == characteristicItem.CharacteristicValue1));
                                    }
                                    break;
                            }
                            break;

                        case "Yes/No":
                            query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                c.CharacteristicValue == characteristicItem.CharacteristicValue1));
                            break;

                        case "Text":
                            switch (characteristicItem.Comparator)
                            {
                                case "contains":
                                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                                    {
                                        query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                            EF.Functions.Like(c.CharacteristicValue, $"%{characteristicItem.CharacteristicValue1}%")));
                                    }
                                    break;
                                default:
                                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                                    {
                                        query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                                            c.CharacteristicValue == characteristicItem.CharacteristicValue1));
                                    }
                                    break;
                            }
                            break;
                    }
                }
            }

            query = criteria.SortBy?.ToLower() switch
            {
                "avnumber" => criteria.Descending ? query.OrderByDescending(i => i.Avnumber) : query.OrderBy(i => i.Avnumber),
                "senderreferencenumber" => criteria.Descending ? query.OrderByDescending(i => i.SenderReferenceNumber) : query.OrderBy(i => i.SenderReferenceNumber),
                "sampletypename" => criteria.Descending ? query.OrderByDescending(i => i.SampleTypeName) : query.OrderBy(i => i.SampleTypeName),
                "familyname" => criteria.Descending ? query.OrderByDescending(i => i.FamilyName) : query.OrderBy(i => i.FamilyName),
                "typename" => criteria.Descending ? query.OrderByDescending(i => i.TypeName) : query.OrderBy(i => i.TypeName),
                "groupspeciesname" => criteria.Descending ? query.OrderByDescending(i => i.GroupSpeciesName) : query.OrderBy(i => i.GroupSpeciesName),
                "breedname" => criteria.Descending ? query.OrderByDescending(i => i.BreedName) : query.OrderBy(i => i.BreedName),
                "yearofisolation" => criteria.Descending ? query.OrderByDescending(i => i.YearOfIsolation) : query.OrderBy(i => i.YearOfIsolation),
                "receiveddate" => criteria.Descending ? query.OrderByDescending(i => i.ReceivedDate) : query.OrderBy(i => i.ReceivedDate),
                "countryoforiginname" => criteria.Descending ? query.OrderByDescending(i => i.CountryOfOriginName) : query.OrderBy(i => i.CountryOfOriginName),
                "materialtransferagreement" => criteria.Descending ? query.OrderByDescending(i => i.MaterialTransferAgreement) : query.OrderBy(i => i.MaterialTransferAgreement),
                "noofaliquots" => criteria.Descending ? query.OrderByDescending(i => i.NoOfAliquots) : query.OrderBy(i => i.NoOfAliquots),
                "freezername" => criteria.Descending ? query.OrderByDescending(i => i.FreezerName) : query.OrderBy(i => i.FreezerName),
                "trayname" => criteria.Descending ? query.OrderByDescending(i => i.TrayName) : query.OrderBy(i => i.TrayName),
                "well" => criteria.Descending ? query.OrderByDescending(i => i.Well) : query.OrderBy(i => i.Well),
                _ => query
            };
            return query;            
        }

        public async Task<List<IsolateFullDetailsResult>> GetIsolateSearchExportResultAsync(PaginationParameters<SearchCriteria> criteria)
        {
            List<IsolateFullDetailsResult> isolateFullDetailsRecords = new List<IsolateFullDetailsResult>();
            IQueryable<IsolateSearchResult> query = FetchIsolateSearchRecordsAsync(criteria);
            var isolateRecords = await query.ToListAsync();
            foreach (var record in isolateRecords) 
            {
                IsolateFullDetailsResult data = await GetIsolateFullDetailsById(record.IsolateId);
                isolateFullDetailsRecords.Add(data);
            }
            return isolateFullDetailsRecords;
        }

        public async Task<IsolateFullDetailsResult> GetIsolateFullDetailsById(Guid isolateId)
        {
            IsolateFullDetailsResult isolateSearchExportResult = new IsolateFullDetailsResult();
            DataSet dsIsolateData = new DataSet();
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

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dsIsolateData);
                    }                    
                }              
            }
            isolateSearchExportResult = GetIsolateFullDetailsResultsInModel(dsIsolateData);
            return isolateSearchExportResult;
        }

        private IsolateFullDetailsResult GetIsolateFullDetailsResultsInModel(DataSet dataSet)
        {
            IsolateFullDetailsResult isolateFullDetails = new IsolateFullDetailsResult();

            if (dataSet.Tables.Count >= 4)
            {
                DataTable isolateTable = dataSet.Tables[0];
                DataTable dispatchTable = dataSet.Tables[1];
                DataTable viabilityTable = dataSet.Tables[2];
                DataTable characteristicTable = dataSet.Tables[3];

                foreach (DataRow isolateRow in isolateTable.Rows)
                {
                    isolateFullDetails.IsolateDetails = new IsolateInfo
                    {
                        Avnumber = isolateRow["AVNumber"].ToString(),
                        FamilyName = isolateRow["FamilyName"].ToString(),
                        TypeName = isolateRow["TypeName"].ToString(),
                        GroupSpeciesName = isolateRow["GroupSpeciesName"].ToString(),
                        BreedName = isolateRow["BreedName"].ToString(),
                        CountryOfOriginName = isolateRow["CountryOfOriginName"].ToString(),
                        YearOfIsolation = isolateRow["YearOfIsolation"] as int?,
                        ReceivedDate = isolateRow["ReceivedDate"] as DateTime?,
                        FreezerName = isolateRow["FreezerName"].ToString(),
                        TrayName = isolateRow["TrayName"].ToString(),
                        Well = isolateRow["Well"].ToString(),
                        MaterialTransferAgreement = Convert.ToBoolean(isolateRow["MaterialTransferAgreement"]),
                        NoOfAliquots = isolateRow["NoOfAliquots"] as int?,
                        IsolateId = (Guid)isolateRow["IsolateID"],
                        SenderReferenceNumber = isolateRow["SenderReferenceNumber"].ToString(),
                        IsolationMethodName = isolateRow["IsolationMethodName"].ToString(),
                        AntiserumProduced = Convert.ToBoolean(isolateRow["AntiserumProduced"]),
                        AntigenProduced = Convert.ToBoolean(isolateRow["AntigenProduced"]),
                        PhylogeneticAnalysis = isolateRow["PhylogeneticAnalysis"].ToString(),
                        PhylogeneticFileName = isolateRow["PhylogeneticFileName"].ToString(),
                        Mtalocation = isolateRow["MTALocation"].ToString(),
                        Comment = isolateRow["Comment"].ToString(),
                        ValidToIssue = isolateRow["ValidToIssue"] as bool?,
                        WhyNotValidToIssue = isolateRow["WhyNotValidToIssue"].ToString(),
                        OriginalSampleAvailable = Convert.ToBoolean(isolateRow["OriginalSampleAvailable"]),
                        FirstViablePassageNumber = isolateRow["FirstViablePassageNumber"] as int?,
                        IsMixedIsolate = Convert.ToBoolean(isolateRow["IsMixedIsolate"]),
                        Nomenclature = isolateRow["Nomenclature"].ToString(),
                        SmsreferenceNumber = isolateRow["SMSReferenceNumber"].ToString(),
                        HostPurposeName = isolateRow["HostPurposeName"].ToString(),
                        SampleTypeName = isolateRow["SampleTypeName"].ToString()
                    };
                }
                // Fill IsolateDispatchDetails
                List<IsolateDispatchInfo> dispatchInfos = new List<IsolateDispatchInfo>();
                foreach (DataRow dispatchRow in dispatchTable.Rows)
                {
                    dispatchInfos.Add(new IsolateDispatchInfo
                    {
                        NoOfAliquots = Convert.ToInt32(dispatchRow["NoOfAliquots"]),
                        PassageNumber = Convert.ToInt32(dispatchRow["PassageNumber"]),
                        RecipientName = dispatchRow["RecipientName"].ToString(),
                        RecipientAddress = dispatchRow["RecipientAddress"].ToString(),
                        ReasonForDispatch = dispatchRow["ReasonForDispatch"].ToString(),
                        DispatchedDate = Convert.ToDateTime(dispatchRow["DispatchedDate"]),
                        DispatchedByName = dispatchRow["DispatchedByName"].ToString(),
                        DispatchIsolateId = isolateFullDetails.IsolateDetails.IsolateId // Assuming it's the same as IsolateId
                    });
                }
                // Fill IsolateViabilityDetails
                List<IsolateViabilityInfo> viabilityInfos = new List<IsolateViabilityInfo>();
                foreach (DataRow viabilityRow in viabilityTable.Rows)
                {
                    viabilityInfos.Add(new IsolateViabilityInfo
                    {
                        ViabilityStatus = viabilityRow["ViabilityStatus"].ToString(),
                        DateChecked = Convert.ToDateTime(viabilityRow["DateChecked"]),
                        CheckedByName = viabilityRow["CheckedByName"].ToString(),
                        IsolateViabilityIsolateId = isolateFullDetails.IsolateDetails.IsolateId // Assuming it's the same as IsolateId
                    });
                }
                // Fill IsolateCharacteristicDetails
                List<IsolateCharacteristicInfo> characteristicInfos = new List<IsolateCharacteristicInfo>();
                foreach (DataRow characteristicRow in characteristicTable.Rows)
                {
                    characteristicInfos.Add(new IsolateCharacteristicInfo
                    {
                        CharacteristicId = (Guid)characteristicRow["CharacteristicId"],
                        CharacteristicName = characteristicRow["CharacteristicName"].ToString(),
                        CharacteristicValue = characteristicRow["CharacteristicValue"].ToString(),
                        CharacteristicPrefix = characteristicRow["CharacteristicPrefix"].ToString(),
                        IsolateId = isolateFullDetails.IsolateDetails.IsolateId
                    });
                }                
                isolateFullDetails.IsolateDispatchDetails = dispatchInfos;
                isolateFullDetails.IsolateViabilityDetails = viabilityInfos;
                isolateFullDetails.IsolateCharacteristicDetails = characteristicInfos;
            }
            return isolateFullDetails;
        }
    }
}
