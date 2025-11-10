using KafeQRMenu.Data.Core.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Entities
{
    public class MenuItem : AuditableEntity
    {
        public string MenuItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Guid MenuCategoryId { get; set; }
        public virtual MenuCategory MenuCategory { get; set; }
        public int SortOrder { get; set; }
        public Guid? MenuItemImageId { get; set; }
        public virtual ImageFile MenuItemImage { get; set; }
    }
}
