using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.Bridge.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.ImageRepositories
{
    public interface IImageFileRepository : IAsyncDeletableRepository<ImageFile>, IAsyncFindable<ImageFile>, IAsyncInsertable<ImageFile>, IAsyncOrderable<ImageFile>, IAsyncQueryable<ImageFile>, IAsyncRepository, IAsyncUpdatable<ImageFile>, IAsyncTransactionable
    {

    }
}
