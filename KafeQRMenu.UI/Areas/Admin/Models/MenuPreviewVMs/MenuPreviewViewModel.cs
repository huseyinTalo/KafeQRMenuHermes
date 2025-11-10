namespace KafeQRMenu.UI.Areas.Admin.Models.MenuPreviewVMs
{
    public class MenuPreviewViewModel
    {
        public string CafeName { get; set; }
        public List<MenuPreviewCategoryVM> Categories { get; set; } = new();
    }
}
