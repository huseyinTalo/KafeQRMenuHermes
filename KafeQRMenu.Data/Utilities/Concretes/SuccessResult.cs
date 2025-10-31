using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Utilities.Concretes
{
    public class SuccessResult : Result
    {
        public SuccessResult(): base(true)
        {
            
        }

        public SuccessResult(string message) : base(true, message)
        {
            
        }
    }
}
