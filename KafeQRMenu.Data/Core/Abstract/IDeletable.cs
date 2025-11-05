using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Core.Abstract
{
    public interface IDeletable
    {
        public string? DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
}
