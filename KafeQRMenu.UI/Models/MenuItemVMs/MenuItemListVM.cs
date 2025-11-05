namespace KafeQRMenu.UI.Models.MenuItemVMs
{
    public class MenuItemListVM
    {
        public Guid MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
