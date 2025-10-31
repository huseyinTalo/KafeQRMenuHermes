using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Core.Abstract
{
    public interface IUpdatable : ICreatable
    {
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
