using KafeQRMenu.Data.Core.Abstract;
using KafeQRMenu.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Core.Concrete
{
    public class AuditableEntity : BaseEntity, IDeletable
    {
        public string? DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
}
