using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.Bridge.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.AdminRepositories
{
    public interface IAdminRepository : IAsyncDeletableRepository<Admin>, IAsyncFindable<Admin>, IAsyncInsertable<Admin>, IAsyncOrderable<Admin>, IAsyncQueryable<Admin>, IAsyncRepository, IAsyncUpdatable<Admin>, IAsyncTransactionable
    {
    }
}
