namespace KafeQRMenu.UI.Models.MenuCategoryVMs
{
    public class MenuCategoryListVM
    {
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }
}
