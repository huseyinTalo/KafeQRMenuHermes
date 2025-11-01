using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.AppContext;
using KafeQRMenu.DataAccess.Bridge.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories
{
    public class MenuCategoryRepository : EFBaseRepository<MenuCategory>, IMenuCategoryRepository
    {
        public MenuCategoryRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
