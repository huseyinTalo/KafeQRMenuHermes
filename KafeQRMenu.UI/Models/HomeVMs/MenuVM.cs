using KafeQRMenu.UI.Models.CafeVMs;
using KafeQRMenu.UI.Models.MenuCategoryVMs;
using KafeQRMenu.UI.Models.MenuItemVMs;

namespace KafeQRMenu.UI.Models.HomeVMs
{
    public class MenuVM
    {
        public CafeVM Cafe { get; set; } = new();
        public List<MenuCategoryListVM> Categories { get; set; } = new();
        public List<MenuItemListVM> Products { get; set; } = new();
    }
}
