namespace KafeQRMenu.UI.Areas.Admin.Models.MenuPreviewVMs
{
    public class MenuPreviewCategoryVM
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Description { get; set; }
        public string? ImageBase64 { get; set; }
        public int SortOrder { get; set; }
        public List<MenuPreviewItemVM> Items { get; set; } = new();
    }
}
