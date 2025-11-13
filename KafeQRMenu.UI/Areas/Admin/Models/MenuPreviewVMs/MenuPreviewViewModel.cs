namespace KafeQRMenu.UI.Areas.Admin.Models.MenuPreviewVMs
{
    public class MenuPreviewViewModel
    {
        public string CafeName { get; set; }
        public string MenuName { get; set; }
        public Guid? SelectedMenuId { get; set; } // Seçili menü
        public List<MenuSelectionItem> AvailableMenus { get; set; } = new(); // Tüm menüler
        public List<MenuPreviewCategoryVM> Categories { get; set; } = new();
    }

    public class MenuSelectionItem
    {
        public Guid MenuId { get; set; }
        public string MenuName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
