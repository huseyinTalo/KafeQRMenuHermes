namespace KafeQRMenu.UI.Areas.Admin.Models.MenuCategoryVMs
{
    public class MenuCategoryListViewModel
    {
        public List<MenuCategoryItemViewModel> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public int SortOrder { get; set; }
        public string MenuCategoryName { get; set; }
        public string Description { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public Guid MenuCategoryId { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[] ImageFileBytes { get; set; }
    }
}
