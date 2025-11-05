namespace KafeQRMenu.UI.Areas.SuperAdmin.Models.MenuItemVMs
{
    public class SAMenuItemVM
    {
        public Guid MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int SortOrder { get; set; }
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
    }
}
