using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Core.Abstract
{
    public interface ICreatable : IBaseEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
