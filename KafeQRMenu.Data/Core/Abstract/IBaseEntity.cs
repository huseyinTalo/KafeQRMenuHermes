using KafeQRMenu.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Core.Abstract
{
    public interface IBaseEntity
    {
        public Guid Id { get; set; }

        public Status Status { get; set; }
    }
}
