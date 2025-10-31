using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Utilities.Concretes
{
    public class SuccessDataResult<T> : DataResult<T> where T : class
    {
        public SuccessDataResult() : base (default, true)
        {
            
        }
        public SuccessDataResult(T data) : base(data, true)
        {
            
        }

        public SuccessDataResult(T data, string message) : base(data, true, message)
        {
            
        }
    }
}
