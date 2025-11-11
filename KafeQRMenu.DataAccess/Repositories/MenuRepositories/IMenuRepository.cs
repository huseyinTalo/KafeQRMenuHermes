using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.Bridge.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.MenuRepositories
{
    public interface IMenuRepository : IAsyncDeletableRepository<Menu>, IAsyncFindable<Menu>, IAsyncInsertable<Menu>, IAsyncOrderable<Menu>, IAsyncQueryable<Menu>, IAsyncRepository, IAsyncUpdatable<Menu>, IAsyncTransactionable
    {
        Task<Menu?> GetByIdWithCategoriesAsync(Guid menuId);
        Task<List<Menu>> GetAllWithDetailsAsync(Guid cafeId);
        Task<List<Menu>> GetAllWithCafeAsync();
    }
}
