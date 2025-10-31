using KafeQRMenu.Data.Core.Concrete;
using KafeQRMenu.DataAccess.Bridge.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Bridge.EntityFramework
{
    public class EFBaseRepository<TEntity> : IRepository, IAsyncDeletableRepository<TEntity>, IAsyncRepository, IAsyncFindable<TEntity>, IAsyncOrderable<TEntity>, IAsyncTransactionable, IAsyncUpdatable<TEntity>, IAsyncInsertable<TEntity>, IAsyncQueryable<TEntity> where TEntity : BaseEntity
    {
        public Task<TEntity> AddAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> tities)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            throw new NotImplementedException();
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IExecutionStrategy> CreateExecutionStrategy()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRangeAsync(IEnumerable<TEntity> tities)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> GetAllAsync<TKey>(Expression<Func<TEntity, TKey>> orderBy, bool orderByDescending, bool tracking = true)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> GetAllAsync<TKey>(Expression<Func<TEntity, bool>> expression, Expression<Func<TEntity, TKey>> orderBy, bool orderByDescending, bool tracking = true)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> GetAllAsync(bool tracking = true)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> expression, bool tracking = true)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression = null, bool tracking = true)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetById(Guid id, bool tracking = true)
        {
            throw new NotImplementedException();
        }

        public int SaveChange()
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
