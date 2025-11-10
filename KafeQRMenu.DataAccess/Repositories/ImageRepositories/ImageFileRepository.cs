using KafeQRMenu.Data.Entities;
using KafeQRMenu.DataAccess.AppContext;
using KafeQRMenu.DataAccess.Bridge.EntityFramework;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
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
    }
}
