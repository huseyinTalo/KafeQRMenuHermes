using KafeQRMenu.Data.Core.Concrete;
using KafeQRMenu.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Entities
{
    public class ImageFile : AuditableEntity
    {
        public byte[] ImageByteFile { get; set; }
        public bool IsActive { get; set; }
        public Guid? MenuCategoryId { get; set; }
        public Guid? MenuItemId { get; set; }
        public virtual MenuItem MenuItem { get; set; }
        public Guid? AdminId { get; set; }
        public virtual Admin Admin { get; set; }
        public Guid? SuperAdminId { get; set; }
        public Guid? CafeId { get; set; }
        public Guid? MenuId { get; set; }
        public ImageContentType ImageContentType { get; set; }
    }
}
