using KafeQRMenu.Data.Core.Abstract;
using KafeQRMenu.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Core.Concrete
{
    public class BaseEntity : IUpdatable
    {
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public Guid Id { get; set; }
        public Status Status { get; set; }
    }
}
