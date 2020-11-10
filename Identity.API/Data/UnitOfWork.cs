using CommonUtil;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.API.Data
{
    public class UnitOfWork : IDisposable
    {
        internal readonly IdentityDBContext _dbContext;

        private bool _isAlive = true;

        public UnitOfWork(IdentityDBContext dbContext) => _dbContext = dbContext;

        internal Maybe<T> GetById<T>(object id) where T : class
        {
            DbSet<T> table = _dbContext.Set<T>();
            return table.Find(id);
        }

        internal void Insert<T>(T obj) where T : class
        {
            DbSet<T> table = _dbContext.Set<T>();
            table.Add(obj);
        }

        internal void Insert<T>(IEnumerable<T> objs) where T : class
        {
            DbSet<T> table = _dbContext.Set<T>();
            table.AddRangeAsync(objs);
        }

        internal void Update<T>(T obj) where T : class
        {
            DbSet<T> table = _dbContext.Set<T>();
            table.Attach(obj);
            _dbContext.Entry(obj).State = EntityState.Modified;
        }

        internal void Update<T>(IEnumerable<T> objs) where T : class
        {
            DbSet<T> table = _dbContext.Set<T>();
            table.AttachRange(objs);
            objs.ToList().ForEach(obj => _dbContext.Entry(obj).State = EntityState.Modified);
        }

        internal void Delete<T>(object id) where T : class
        {
            DbSet<T> table = _dbContext.Set<T>();
            T existing = table.Find(id);
            table.Remove(existing);
        }

        internal void Delete<T>(IEnumerable<T> entities) where T : class
        {
            DbSet<T> table = _dbContext.Set<T>();
            table.RemoveRange(entities);
        }

        internal IQueryable<T> Query<T>(bool noTracking = true) where T : class
        {
            DbSet<T> table = _dbContext.Set<T>();
            return noTracking ? table.AsQueryable<T>().AsNoTracking() : table.AsQueryable<T>();
        }

        public bool Commit(ILogger logger)
        {
            if (!_isAlive)
                return true;

            //_isCommitted = true;

            try
            {
                logger.LogInformation("Saving changes to DB...");
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while saving changes to DB");
                return false;
            }
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //_dbContext.Dispose(); // <- automatically dispose by the DI container, commented to avoid ObjectDisposedException
                }
            }
            disposed = true;
        }
        public void Dispose()
        {
            if (!_isAlive)
                return;

            _isAlive = false;

            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
