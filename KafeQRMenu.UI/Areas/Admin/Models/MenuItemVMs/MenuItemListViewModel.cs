namespace KafeQRMenu.UI.Areas.Admin.Models.MenuItemVMs
{
    public class MenuItemListViewModel
    {
        public List<MenuItemViewModel> MenuItems { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public string? SearchTerm { get; set; }
        public Guid? CategoryFilter { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? ImageFileBase64 { get; set; }
    }

}