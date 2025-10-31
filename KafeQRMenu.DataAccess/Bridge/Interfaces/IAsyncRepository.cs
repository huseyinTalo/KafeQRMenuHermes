using KafeQRMenu.Data.Core.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Bridge.Interfaces
{
    public interface IAsyncRepository
    {
        Task<int> SaveChangeAsync();
    }
}
