using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Enums;
using KafeQRMenu.DataAccess.AppContext;
using KafeQRMenu.DataAccess.Bridge.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.MenuRepositories
{
    public class MenuRepository : EFBaseRepository<Menu>, IMenuRepository
    {
        public MenuRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<Menu?> GetByIdWithCategoriesAsync(Guid menuId)
        {
            return await _table
                .Include(m => m.CategoriesOfMenu)
                .Include(m => m.Cafe)
                .FirstOrDefaultAsync(m => m.Id == menuId && m.Status != Status.Deleted);
        }

        public async Task<List<Menu>> GetAllWithDetailsAsync(Guid cafeId)
        {
            return await _table
                .Include(m => m.CategoriesOfMenu)
                    .ThenInclude(c => c.MenuItems)
                .Include(m => m.Cafe)
                .Where(m => m.CafeId == cafeId && m.Status != Status.Deleted)
                .ToListAsync();
        }

        public async Task<List<Menu>> GetAllWithCafeAsync()
        {
            return await _table
                .Include(m => m.Cafe) // ← ADD THIS
                .Where(m => m.Status != Status.Deleted)
                .OrderByDescending(m => m.CreatedTime)
                .ToListAsync();
        }
    }
}
