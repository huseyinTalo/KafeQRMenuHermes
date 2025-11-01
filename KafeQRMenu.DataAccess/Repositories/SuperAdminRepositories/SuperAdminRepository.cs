using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.AppContext;
using KafeQRMenu.DataAccess.Bridge.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.SuperAdminRepositories
{
    public class SuperAdminRepository : EFBaseRepository<SuperAdmin>, ISuperAdminRepository
    {
        public SuperAdminRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
