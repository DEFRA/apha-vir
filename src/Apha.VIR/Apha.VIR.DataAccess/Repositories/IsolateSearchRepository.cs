using System.Collections;
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

            query = (IQueryable<IsolateSearchResult>)ApplyBasicFilters(query, criteria.Filter);

            query = (IQueryable<IsolateSearchResult>)ApplyDateFilters(query, criteria.Filter);

            query = (IQueryable<IsolateSearchResult>)ApplyAllCharacteristicFilters(query, criteria.Filter.CharacteristicSearch);            

            query = (IQueryable<IsolateSearchResult>)ApplySorting(query, criteria.SortBy, criteria.Descending);
            
            return query;            
        }
       
        private static IQueryable ApplyBasicFilters(IQueryable<IsolateSearchResult> query, SearchCriteria? filter)
        {
            if(filter != null)
            {
                if (!string.IsNullOrEmpty(filter.AVNumber))
                {
                    query = query.Where(i => i.Avnumber == filter.AVNumber);
                }

                if (IsValidGuid(filter.VirusFamily))
                {
                    query = query.Where(i => i.Family == filter.VirusFamily);
                }

                if (IsValidGuid(filter.VirusType))
                {
                    query = query.Where(i => i.Type == filter.VirusType);
                }

                if (IsValidGuid(filter.Group))
                {
                    query = query.Where(i => i.HostSpecies == filter.Group);
                }

                if (IsValidGuid(filter.Species))
                {
                    query = query.Where(i => i.HostBreed == filter.Species);
                }

                if (IsValidGuid(filter.CountryOfOrigin))
                {
                    query = query.Where(i => i.CountryOfOrigin == filter.CountryOfOrigin);
                }

                if (IsValidGuid(filter.HostPurpose))
                {
                    query = query.Where(i => i.HostPurpose == filter.HostPurpose);
                }

                if (IsValidGuid(filter.SampleType))
                {
                    query = query.Where(i => i.SampleType == filter.SampleType);
                }

                if (filter.YearOfIsolation != 0)
                {
                    query = query.Where(i => i.YearOfIsolation == filter.YearOfIsolation);
                }
            }   
            return query;
        }

        private static IQueryable ApplyDateFilters(IQueryable<IsolateSearchResult> query, SearchCriteria? filter)
        {
            if(filter != null)
            {
                if (filter.ReceivedFromDate.HasValue && filter.ReceivedToDate.HasValue)
                {
                    query = query.Where(i => i.ReceivedDate >= filter.ReceivedFromDate && i.ReceivedDate <= filter.ReceivedToDate);
                }

                if (filter.CreatedFromDate.HasValue && filter.CreatedToDate.HasValue)
                {
                    query = query.Where(i => i.DateCreated >= filter.CreatedFromDate && i.DateCreated <= filter.CreatedToDate);
                }
            }     
            return query;
        }

        private IQueryable ApplyAllCharacteristicFilters(IQueryable<IsolateSearchResult> query, List<CharacteristicCriteria> characteristicSearch)
        {
            foreach (CharacteristicCriteria characteristicItem in characteristicSearch)
            {
                if (IsValidGuid(characteristicItem.Characteristic))
                {
                    query = (IQueryable<IsolateSearchResult>)ApplyCharacteristicFilter(query, characteristicItem);
                }
            }
            return query;
        }

        private IQueryable ApplyCharacteristicFilter(IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
            query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                         c.CharacteristicIsolateId == i.IsolateId && c.VirusCharacteristicId == characteristicItem.Characteristic));

            switch (characteristicItem.CharacteristicType)
            {
                case "Numeric":
                    query = (IQueryable<IsolateSearchResult>)ApplyNumericCharacteristicFilter(query, characteristicItem);
                    break;

                case "SingleList":
                    query = (IQueryable<IsolateSearchResult>)ApplySingleListCharacteristicFilter(query, characteristicItem);
                    break;

                case "Yes/No":
                    query = (IQueryable<IsolateSearchResult>)ApplyYesNoCharacteristicFilter(query, characteristicItem);
                    break;

                case "Text":
                    query = (IQueryable<IsolateSearchResult>)ApplyTextCharacteristicFilter(query, characteristicItem);
                    break;
            }
            return query;
        }
        
        private IQueryable ApplyNumericCharacteristicFilter(IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
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
            return query;
        }

        private IQueryable ApplySingleListCharacteristicFilter(IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
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
            return query;
        }

        private IQueryable ApplyYesNoCharacteristicFilter(IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
            query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                        c.CharacteristicValue == characteristicItem.CharacteristicValue1));
            return query;
        }

        private IQueryable ApplyTextCharacteristicFilter(IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
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
            return query;
        }

        private static IQueryable ApplySorting(IQueryable<IsolateSearchResult> query, string? sortBy, bool descending)
        {           
            return sortBy?.ToLower() switch
            {
                "avnumber" => descending ? query.OrderByDescending(i => i.Avnumber) : query.OrderBy(i => i.Avnumber),
                "senderreferencenumber" => descending ? query.OrderByDescending(i => i.SenderReferenceNumber) : query.OrderBy(i => i.SenderReferenceNumber),
                "sampletypename" => descending ? query.OrderByDescending(i => i.SampleTypeName) : query.OrderBy(i => i.SampleTypeName),
                "familyname" => descending ? query.OrderByDescending(i => i.FamilyName) : query.OrderBy(i => i.FamilyName),
                "typename" => descending ? query.OrderByDescending(i => i.TypeName) : query.OrderBy(i => i.TypeName),
                "groupspeciesname" => descending ? query.OrderByDescending(i => i.GroupSpeciesName) : query.OrderBy(i => i.GroupSpeciesName),
                "breedname" => descending ? query.OrderByDescending(i => i.BreedName) : query.OrderBy(i => i.BreedName),
                "yearofisolation" => descending ? query.OrderByDescending(i => i.YearOfIsolation) : query.OrderBy(i => i.YearOfIsolation),
                "receiveddate" => descending ? query.OrderByDescending(i => i.ReceivedDate) : query.OrderBy(i => i.ReceivedDate),
                "countryoforiginname" => descending ? query.OrderByDescending(i => i.CountryOfOriginName) : query.OrderBy(i => i.CountryOfOriginName),
                "materialtransferagreement" => descending ? query.OrderByDescending(i => i.MaterialTransferAgreement) : query.OrderBy(i => i.MaterialTransferAgreement),
                "noofaliquots" => descending ? query.OrderByDescending(i => i.NoOfAliquots) : query.OrderBy(i => i.NoOfAliquots),
                "freezername" => descending ? query.OrderByDescending(i => i.FreezerName) : query.OrderBy(i => i.FreezerName),
                "trayname" => descending ? query.OrderByDescending(i => i.TrayName) : query.OrderBy(i => i.TrayName),
                "well" => descending ? query.OrderByDescending(i => i.Well) : query.OrderBy(i => i.Well),
                _ => query
            };
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

        public static bool IsValidGuid(Guid? guid)
        {
            return guid.HasValue && guid.Value != Guid.Empty;
        }
    }
}
