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
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string DeletedBy { get; set; }
        public DateTime DeletedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public Status Status { get; set; }
    }
}
