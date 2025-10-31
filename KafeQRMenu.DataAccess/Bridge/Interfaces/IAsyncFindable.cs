using KafeQRMenu.Data.Core.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Bridge.Interfaces
{
    public interface IAsyncFindable<TEntity> where TEntity : BaseEntity
    {
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression = null);
        Task<TEntity> GetById(Guid id, bool tracking = true);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression = null, bool tracking = true);
    }
}
