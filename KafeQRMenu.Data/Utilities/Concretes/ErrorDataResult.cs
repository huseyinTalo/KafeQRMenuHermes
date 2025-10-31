using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Utilities.Concretes
{
    public class ErrorDataResult<T> : DataResult<T> where T: class
    {
        public ErrorDataResult() : base(default, true)
        {
            
        }
        public ErrorDataResult(T data) : base(data, false)
        {
            
        }

        public ErrorDataResult(T data, string message) : base(data, false, message)
        {
            
        }
    }
}
