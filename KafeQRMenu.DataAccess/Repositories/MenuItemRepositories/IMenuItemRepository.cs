using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.Bridge.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.MenuItemRepositories
{
    public interface IMenuItemRepository : IAsyncDeletableRepository<MenuItem>, IAsyncFindable<MenuItem>, IAsyncInsertable<MenuItem>, IAsyncOrderable<MenuItem>, IAsyncQueryable<MenuItem>, IAsyncRepository, IAsyncUpdatable<MenuItem>, IAsyncTransactionable
    {
    }
}
