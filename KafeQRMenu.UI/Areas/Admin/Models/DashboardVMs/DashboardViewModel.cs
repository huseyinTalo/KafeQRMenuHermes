using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;

namespace KafeQRMenu.UI.Areas.Admin.Models.DashboardVMs
{
    public class DashboardViewModel
    {
        public string AdminName { get; set; }
        public string CafeName { get; set; }

        // Statistics
        public int TotalCategories { get; set; }
        public int TotalMenuItems { get; set; }
        public decimal AveragePrice { get; set; }
        public int QRViewsToday { get; set; }
        public int CategoriesThisMonth { get; set; }
        public int MenuItemsThisMonth { get; set; }

        // Recent Items
        public List<RecentCategoryDTO> RecentCategories { get; set; } = new();
        public List<RecentMenuItemDTO> RecentMenuItems { get; set; } = new();

        // Chart Data
        public List<string> CategoryNames { get; set; } = new();
        public List<int> CategoryItemCounts { get; set; } = new();
    }
}
