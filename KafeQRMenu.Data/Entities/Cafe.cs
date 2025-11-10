using KafeQRMenu.Data.Core.Concrete;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Entities
{
    public class Cafe : AuditableEntity
    {
        public string CafeName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public virtual ICollection<Admin> Admins { get; set; } = new HashSet<Admin>();
        public virtual ICollection<MenuCategory> MenuCategories { get; set; } = new HashSet<MenuCategory>();
        public virtual ICollection<Menu> Menus { get; set; } = new HashSet<Menu>();
        public Guid? ImageFileId { get; set; }
    }
}
