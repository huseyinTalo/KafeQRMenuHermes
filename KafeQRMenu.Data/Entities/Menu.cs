using KafeQRMenu.Data.Core.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Entities
{
    public class Menu : AuditableEntity
    {
        public string MenuName { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<MenuCategory> CategoriesOfMenu { get; set; } = new HashSet<MenuCategory>();
        public Guid? ImageFileId { get; set; }
        public Guid CafeId { get; set; }
        public virtual Cafe Cafe { get; set; }
    }
}
