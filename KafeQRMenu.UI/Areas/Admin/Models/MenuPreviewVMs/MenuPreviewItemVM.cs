namespace KafeQRMenu.UI.Areas.Admin.Models.MenuPreviewVMs
{
    public class MenuPreviewItemVM
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
