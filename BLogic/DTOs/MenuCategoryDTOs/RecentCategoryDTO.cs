using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs
{
    public class RecentCategoryDTO
    {
        public Guid Id { get; set; }
        public string MenuCategoryName { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
