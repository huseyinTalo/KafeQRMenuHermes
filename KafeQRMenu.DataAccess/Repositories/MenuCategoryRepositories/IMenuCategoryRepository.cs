using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.Bridge.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories
{
    public interface IMenuCategoryRepository : IAsyncDeletableRepository<MenuCategory>, IAsyncFindable<MenuCategory>, IAsyncInsertable<MenuCategory>, IAsyncOrderable<MenuCategory>, IAsyncQueryable<MenuCategory>, IAsyncRepository, IAsyncUpdatable<MenuCategory>, IAsyncTransactionable
    {
    }
}
