using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories
{
    public class RepositoryBase<TEntity> where TEntity : class
    {
        protected readonly VIRDbContext _context;

        public RepositoryBase(VIRDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected virtual IQueryable<TEntity> GetQueryableResult(string sql, params object[] parameters)
        {
            return _context.Set<TEntity>().FromSqlRaw(sql, parameters);
        }
        // allows querying for any arbitrary type (like VirusCharacteristic, VirusCharacteristicListEntry, etc.)
        protected virtual IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters) where T : class
        {
            return _context.Set<T>().FromSqlRaw(sql, parameters);
        }

        protected virtual IQueryable<T> GetDbSetFor<T>() where T : class
        {
            return _context.Set<T>();
        }
        //Interpolated SQL generic method
        protected virtual IQueryable<T> GetQueryableInterpolatedFor<T>(FormattableString sql) where T : class
        {
            return _context.Set<T>().FromSqlInterpolated(sql);
        }

        protected virtual Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql)
        {
            return _context.Database.ExecuteSqlInterpolatedAsync(sql);
        }

    }
}
