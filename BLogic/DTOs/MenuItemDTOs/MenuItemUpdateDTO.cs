using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.MenuItemDTOs
{
    public class MenuItemUpdateDTO
    {
        public Guid MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int SortOrder { get; set; }
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
        public Guid? ImageFileId { get; set; }
    }
}
