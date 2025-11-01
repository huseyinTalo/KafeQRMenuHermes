using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.DTOs.CafeDTOs
{
    public class CafeUpdateDTO
    {
        public Guid Id { get; set; }
        public string CafeName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
    }
}
