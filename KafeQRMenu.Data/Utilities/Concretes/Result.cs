using KafeQRMenu.Data.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.Data.Utilities.Concretes
{
    public class Result : IResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public Result()
        {
            IsSuccess = false;
            Message = string.Empty;
        }
        public Result(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public Result(bool isSuccess, string message) : this(isSuccess)
        {
            Message = message;
        }
    }
}
