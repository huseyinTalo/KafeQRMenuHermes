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
        /// <summary>
        /// Fetches multiple ImageFiles by their IDs in a single query to avoid N+1 problems.
        /// </summary>
        /// <param name="ids">Collection of ImageFile IDs to fetch</param>
        /// <param name="tracking">Whether to track the entities (default: true)</param>
        /// <returns>Dictionary mapping ImageFile IDs to their entities</returns>
        Task<Dictionary<Guid, ImageFile>> GetByIdsAsync(IEnumerable<Guid> ids, bool tracking = true);
    }
}
