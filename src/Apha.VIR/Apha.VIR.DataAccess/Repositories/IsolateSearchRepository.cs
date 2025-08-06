using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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

            if (criteria.Filter != null)
                query = (IQueryable<IsolateSearchResult>)ApplyAllCharacteristicFilters(query, criteria.Filter.CharacteristicSearch);

            query = (IQueryable<IsolateSearchResult>)ApplySorting(query, criteria.SortBy, criteria.Descending);

            return query;
        }

        private static IQueryable ApplyBasicFilters(IQueryable<IsolateSearchResult> query, SearchCriteria? filter)
        {
            if (filter == null)
            {
                return query;
            }
            if (!String.IsNullOrEmpty(filter.AVNumber))
                query = ApplyStringFilter(query, filter.AVNumber, i => i.Avnumber);
            query = ApplyGuidFilters(query, filter);
            query = ApplyYearOfIsolationFilter(query, filter.YearOfIsolation);

            return query;
        }
       
        private static IQueryable<IsolateSearchResult> ApplyStringFilter(
            IQueryable<IsolateSearchResult> query,
            string filterValue,
            Expression<Func<IsolateSearchResult, string>> propertySelector)
        {
            if (string.IsNullOrEmpty(filterValue))
                return query;
            var parameter = propertySelector.Parameters[0];
            var constant = Expression.Constant(filterValue, typeof(string));
            var equality = Expression.Equal(propertySelector.Body, constant);
            var lambda = Expression.Lambda<Func<IsolateSearchResult, bool>>(equality, parameter);
            return query.Where(lambda);
        }

        private static IQueryable<IsolateSearchResult> ApplyGuidFilters(IQueryable<IsolateSearchResult> query, SearchCriteria filter)
        {
            query = ApplyGuidFilter(query, filter.VirusFamily, i => i.Family);
            query = ApplyGuidFilter(query, filter.VirusType, i => i.Type);
            query = ApplyGuidFilter(query, filter.Group, i => i.HostSpecies);
            query = ApplyGuidFilter(query, filter.Species, i => i.HostBreed);
            query = ApplyGuidFilter(query, filter.CountryOfOrigin, i => i.CountryOfOrigin);
            query = ApplyGuidFilter(query, filter.HostPurpose, i => i.HostPurpose);
            query = ApplyGuidFilter(query, filter.SampleType, i => i.SampleType);

            return query;
        }

        private static IQueryable<IsolateSearchResult> ApplyGuidFilter(
            IQueryable<IsolateSearchResult> query,
            Guid? filterValue,
            Expression<Func<IsolateSearchResult, Guid?>> propertySelector)
        {
            if (!IsValidGuid(filterValue))
                return query;
            var parameter = propertySelector.Parameters[0];
            var constant = Expression.Constant(filterValue, typeof(string));
            var equality = Expression.Equal(propertySelector.Body, constant);
            var lambda = Expression.Lambda<Func<IsolateSearchResult, bool>>(equality, parameter);
            return query.Where(lambda);
        }

        private static IQueryable<IsolateSearchResult> ApplyYearOfIsolationFilter(IQueryable<IsolateSearchResult> query, int yearOfIsolation)
        {
            return yearOfIsolation == 0
                ? query
                : query.Where(i => i.YearOfIsolation == yearOfIsolation);
        }

        private static IQueryable ApplyDateFilters(IQueryable<IsolateSearchResult> query, SearchCriteria? filter)
        {
            if (filter != null)
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
                            float.Parse(c.CharacteristicValue!) >= float.Parse(characteristicItem.CharacteristicValue1)));
                    }
                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue2))
                    {
                        query = query.Where(i => _context.VwCharacteristicsForSearches.Any(c =>
                            float.Parse(c.CharacteristicValue!) <= float.Parse(characteristicItem.CharacteristicValue2)));
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
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<IsolateSearchResult> query, string property, bool descending)
        {
            return property switch
            {
                "avnumber" => ApplyOrder(query, i => i.Avnumber, descending),
                "senderreferencenumber" => ApplyOrder(query, i => i.SenderReferenceNumber, descending),
                "sampletypename" => ApplyOrder(query, i => i.SampleTypeName, descending),
                "familyname" => ApplyOrder(query, i => i.FamilyName, descending),
                "typename" => ApplyOrder(query, i => i.TypeName, descending),
                "groupspeciesname" => ApplyOrder(query, i => i.GroupSpeciesName, descending),
                "breedname" => ApplyOrder(query, i => i.BreedName, descending),
                "yearofisolation" => ApplyOrder(query, i => i.YearOfIsolation, descending),
                "receiveddate" => ApplyOrder(query, i => i.ReceivedDate, descending),
                "countryoforiginname" => ApplyOrder(query, i => i.CountryOfOriginName, descending),
                "materialtransferagreement" => ApplyOrder(query, i => i.MaterialTransferAgreement, descending),
                "noofaliquots" => ApplyOrder(query, i => i.NoOfAliquots, descending),
                "freezername" => ApplyOrder(query, i => i.FreezerName, descending),
                "trayname" => ApplyOrder(query, i => i.TrayName, descending),
                "well" => ApplyOrder(query, i => i.Well, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<IsolateSearchResult> query, Expression<Func<IsolateSearchResult, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
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
            IsolateFullDetailsResult isolateSearchExportResult = GetIsolateFullDetailsResultsInModel(dsIsolateData);
            return isolateSearchExportResult;
        }

        private IsolateFullDetailsResult GetIsolateFullDetailsResultsInModel(DataSet dataSet)
        {
            IsolateFullDetailsResult isolateFullDetails = new IsolateFullDetailsResult();

            if (dataSet.Tables.Count >= 4)
            {
                isolateFullDetails.IsolateDetails = GetIsolateDetails(dataSet.Tables[0]);
                isolateFullDetails.IsolateDispatchDetails = GetDispatchDetails(dataSet.Tables[1], isolateFullDetails.IsolateDetails!.IsolateId);
                isolateFullDetails.IsolateViabilityDetails = GetViabilityDetails(dataSet.Tables[2], isolateFullDetails.IsolateDetails!.IsolateId);
                isolateFullDetails.IsolateCharacteristicDetails = GetCharacteristicDetails(dataSet.Tables[3], isolateFullDetails.IsolateDetails!.IsolateId);                
            }
            return isolateFullDetails;
        }

        private IsolateInfo? GetIsolateDetails(DataTable isolateTable)
        {
            IsolateInfo? isolateInfo = null;
            var isolateRow = isolateTable.Rows.Cast<DataRow>().FirstOrDefault();
            if (isolateRow != null) {            
                isolateInfo = new IsolateInfo
                {
                    AvNumber = isolateRow["AVNumber"].ToString(),
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
            return isolateInfo;
        }

        private List<IsolateDispatchInfo> GetDispatchDetails(DataTable dispatchTable, Guid isolateId)
        {
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
                    DispatchedByName = dispatchRow["DispatchedByName"] is DBNull ? "" : dispatchRow["DispatchedByName"].ToString()!,
                    DispatchIsolateId = isolateId 
                });
            }
            return dispatchInfos;
        }

        private List<IsolateViabilityInfo> GetViabilityDetails(DataTable viabilityTable, Guid isolateId)
        {
            List<IsolateViabilityInfo> viabilityInfos = new List<IsolateViabilityInfo>();
            foreach (DataRow viabilityRow in viabilityTable.Rows)
            {
                viabilityInfos.Add(new IsolateViabilityInfo
                {
                    ViabilityStatus = viabilityRow["ViabilityStatus"] is DBNull ? "" : viabilityRow["ViabilityStatus"].ToString()!,
                    DateChecked = Convert.ToDateTime(viabilityRow["DateChecked"]),
                    CheckedByName = viabilityRow["CheckedByName"] is DBNull ? "" : viabilityRow["CheckedByName"].ToString()!,
                    IsolateViabilityIsolateId = isolateId 
                });
            }
            return viabilityInfos;
        }

        private List<IsolateCharacteristicInfo> GetCharacteristicDetails(DataTable characteristicTable, Guid isolateId)
        {
            List<IsolateCharacteristicInfo> characteristicInfos = new List<IsolateCharacteristicInfo>();
            foreach (DataRow characteristicRow in characteristicTable.Rows)
            {
                characteristicInfos.Add(new IsolateCharacteristicInfo
                {
                    CharacteristicId = (Guid)characteristicRow["CharacteristicId"],
                    CharacteristicName = characteristicRow["CharacteristicName"] is DBNull ? "" : characteristicRow["CharacteristicName"].ToString()!,
                    CharacteristicValue = characteristicRow["CharacteristicValue"].ToString(),
                    CharacteristicPrefix = characteristicRow["CharacteristicPrefix"].ToString(),
                    IsolateId = isolateId
                });
            }
            return characteristicInfos;
        }

        public static bool IsValidGuid(Guid? guid)
        {
            return guid.HasValue && guid.Value != Guid.Empty;
        }
    }
}
