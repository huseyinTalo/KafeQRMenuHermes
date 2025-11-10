namespace KafeQRMenu.UI.Models.MenuCategoryVMs
{
    public class MenuCategoryListVM
    {
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public string? ImageFileBase64 { get; set; }
    }
}
