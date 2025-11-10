using System;

namespace KafeQRMenu.BLogic.DTOs.MenuItemDTOs
{
    public class MenuItemListDTO
    {
        public Guid MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int SortOrder { get; set; }
        public Guid MenuCategoryId { get; set; }
        public string MenuCategoryName { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[]? ImageFileBytes { get; set; }  // ADD THIS
        public DateTime CreatedTime { get; set; }
    }
}