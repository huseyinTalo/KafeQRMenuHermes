using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.DTOs.MenuItemDTOs
{
    public class MenuItemCreateDTO
    {
        public string MenuItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
