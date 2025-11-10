namespace KafeQRMenu.UI.Areas.Admin.Models.MenuCategoryVMs
{
    public class MenuCategoryItemViewModel
    {
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public string CafeName { get; set; }
        public int ItemCount { get; set; }
        public bool HasImage { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[] ImageFileBytes { get; set; }
    }
}
