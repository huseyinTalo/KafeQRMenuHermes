using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.Bridge.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.SuperAdminRepositories
{
    public interface ISuperAdminRepository : IAsyncDeletableRepository<SuperAdmin>, IAsyncFindable<SuperAdmin>, IAsyncInsertable<SuperAdmin>, IAsyncOrderable<SuperAdmin>, IAsyncQueryable<SuperAdmin>, IAsyncRepository, IAsyncUpdatable<SuperAdmin>, IAsyncTransactionable
    {
    }
}
