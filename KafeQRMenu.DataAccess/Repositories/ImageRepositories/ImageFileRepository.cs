using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Enums;
using KafeQRMenu.DataAccess.AppContext;
using KafeQRMenu.DataAccess.Bridge.EntityFramework;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Repositories.ImageRepositories
{
    public class ImageFileRepository : EFBaseRepository<ImageFile>, IImageFileRepository
    {
        public ImageFileRepository(AppDbContext context) : base(context)
        {

        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, ImageFile>> GetByIdsAsync(IEnumerable<Guid> ids, bool tracking = true)
        {
            if (ids == null || !ids.Any())
            {
                return new Dictionary<Guid, ImageFile>();
            }

            // Remove empty Guids and duplicates
            var validIds = ids.Where(id => id != Guid.Empty).Distinct().ToList();

            if (!validIds.Any())
            {
                return new Dictionary<Guid, ImageFile>();
            }

            var query = _table.Where(img => img.Status != Status.Deleted && validIds.Contains(img.Id));

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            var images = await query.ToListAsync();

            return images.ToDictionary(img => img.Id);
        }
    }
}
