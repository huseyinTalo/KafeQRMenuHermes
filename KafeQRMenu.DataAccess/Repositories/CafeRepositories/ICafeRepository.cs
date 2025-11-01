using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.Bridge.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.CafeRepositories
{
    public interface ICafeRepository : IAsyncDeletableRepository<Cafe>, IAsyncFindable<Cafe>, IAsyncInsertable<Cafe>, IAsyncOrderable<Cafe>, IAsyncQueryable<   Cafe>, IAsyncRepository, IAsyncUpdatable<Cafe>, IAsyncTransactionable
    {
    }
}
