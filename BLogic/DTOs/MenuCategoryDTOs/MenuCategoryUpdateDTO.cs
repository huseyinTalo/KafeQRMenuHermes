using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs
{
    public class MenuCategoryUpdateDTO
    {
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public string CafeName { get; set; }
        public Guid CafeId { get; set; }
    }
}
