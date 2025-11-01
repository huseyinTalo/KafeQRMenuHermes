using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.AppContext;
using KafeQRMenu.DataAccess.Bridge.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.CafeRepositories
{
    public class CafeRepository : EFBaseRepository<Cafe>, ICafeRepository
    {
        public CafeRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
