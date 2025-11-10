using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.MenuItemDTOs
{
    public class RecentMenuItemDTO
    {
        public Guid Id { get; set; }
        public string MenuItemName { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
