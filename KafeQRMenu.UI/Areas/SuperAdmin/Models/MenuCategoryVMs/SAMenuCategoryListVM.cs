namespace KafeQRMenu.UI.Areas.SuperAdmin.Models.MenuCategoryVMs
{
    public class SAMenuCategoryListVM
    {
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public string CafeName { get; set; }
        public Guid CafeId { get; set; }
    }
}
