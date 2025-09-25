using System.Data;
using System.Linq.Expressions;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories
{
    public class IsolateSearchRepository : RepositoryBase<IsolateSearchResult>, IIsolateSearchRepository
    {
        public IsolateSearchRepository(VIRDbContext context) : base(context) { }

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
            var query = GetDbSetFor<IsolateSearchResult>();

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
            var constant = Expression.Constant(filterValue, typeof(Guid?));
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
            var characteristicQuery = GetDbSetFor<IsolateCharacteristicsForSearch>();
            query = query.Where(i => characteristicQuery.Any(c =>
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
            var isolateResult = query.ToList();
            var isolateIds = isolateResult.Count > 0 ? isolateResult.Select(i => i.IsolateId).ToList() : new List<Guid> { Guid.Empty };
            var numericCharacteristics = GetNumericCharacteristicsForSearch(isolateIds, characteristicItem.Characteristic, characteristicItem.CharacteristicValue1, characteristicItem.CharacteristicValue2, characteristicItem.Comparator);
            var charIsolateIds= numericCharacteristics.Select(i => i.CharacteristicIsolateId).ToList();
            query = query.Where(i => charIsolateIds.Contains(i.IsolateId));
            
            return query;
        }

        private IQueryable ApplySingleListCharacteristicFilter(IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
            var characteristicQuery = GetDbSetFor<IsolateCharacteristicsForSearch>();
            switch (characteristicItem.Comparator)
            {
                case "begins with":
                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                    {
                        query = query.Where(i => characteristicQuery.Any(c =>
                            EF.Functions.Like(c.CharacteristicValue, $"{characteristicItem.CharacteristicValue1}%")
                            && i.IsolateId == c.CharacteristicIsolateId
                            && c.VirusCharacteristicId == characteristicItem.Characteristic));
                    }
                    break;
                case "not equal to":
                    query = query.Where(i => characteristicQuery.Any(c =>
                        c.CharacteristicValue != characteristicItem.CharacteristicValue1
                        && i.IsolateId == c.CharacteristicIsolateId
                        && c.VirusCharacteristicId == characteristicItem.Characteristic));
                    break;
                default:
                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                    {
                        query = query.Where(i => characteristicQuery.Any(c =>
                            c.CharacteristicValue == characteristicItem.CharacteristicValue1
                            && i.IsolateId == c.CharacteristicIsolateId
                            && c.VirusCharacteristicId == characteristicItem.Characteristic));
                    }
                    break;
            }
            return query;
        }

        private IQueryable ApplyYesNoCharacteristicFilter(IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
            var characteristicQuery = GetDbSetFor<IsolateCharacteristicsForSearch>();
            query = query.Where(i => characteristicQuery.Any(c =>
                        c.CharacteristicValue == characteristicItem.CharacteristicValue1
                        && i.IsolateId == c.CharacteristicIsolateId
                        && c.VirusCharacteristicId == characteristicItem.Characteristic));
            return query;
        }

        private IQueryable ApplyTextCharacteristicFilter(IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
            var characteristicQuery = GetDbSetFor<IsolateCharacteristicsForSearch>();
            switch (characteristicItem.Comparator)
            {
                case "contains":
                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                    {
                        query = query.Where(i => characteristicQuery.Any(c =>
                            EF.Functions.Like(c.CharacteristicValue, $"%{characteristicItem.CharacteristicValue1}%")
                            && i.IsolateId == c.CharacteristicIsolateId
                            && c.VirusCharacteristicId == characteristicItem.Characteristic));
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(characteristicItem.CharacteristicValue1))
                    {
                        query = query.Where(i => characteristicQuery.Any(c =>
                            c.CharacteristicValue == characteristicItem.CharacteristicValue1
                            && i.IsolateId == c.CharacteristicIsolateId
                            && c.VirusCharacteristicId == characteristicItem.Characteristic));
                    }
                    break;
            }
            return query;
        }

        private List<IsolateCharacteristicsForSearch> GetNumericCharacteristicsForSearch(List<Guid> isolateIds, Guid? characteristic, string? value1, string? value2, string? comparator)
        {
            string sqlQuery = $"SELECT * FROM vwCharacteristicsForSearch INNER JOIN vwIsolate ON vwIsolate.IsolateID = CharacteristicIsolateID " +
                $"WHERE VirusCharacteristicID= '{characteristic}' AND vwIsolate.IsolateID IN ({string.Join(",", isolateIds.Select(id => $"'{id}'"))}) ";
            string whereClause = string.Empty;
            switch (comparator)
            {
                case "between":
                    if (!string.IsNullOrEmpty(value1))
                    {
                        whereClause = $" AND TRY_CAST(CharacteristicValue as float) >= CAST('{value1}' as float)";
                    }
                    if (!string.IsNullOrEmpty(value2))
                    {

                        whereClause += $" AND TRY_CAST(CharacteristicValue as float) <= CAST('{value2}' as float)";
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(value1))
                    {
                        whereClause = $" AND TRY_CAST(CharacteristicValue as float) {comparator} CAST('{value1}' as float)";
                    }
                    break;
            }

            var numericCharacteristics = _context.Database
                .SqlQueryRaw<IsolateCharacteristicsForSearch>(sqlQuery + whereClause)
                .ToList();

            return numericCharacteristics;
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

        public async Task<IEnumerable<IsolateSearchResult>> GetIsolateSearchExportResultAsync(PaginationParameters<SearchCriteria> criteria)
        {
            IQueryable<IsolateSearchResult> query = FetchIsolateSearchRecordsAsync(criteria);
            var isolateRecords = await query.ToListAsync();
            return isolateRecords;
        }

        public static bool IsValidGuid(Guid? guid)
        {
            return guid.HasValue && guid.Value != Guid.Empty;
        }
    }
}
