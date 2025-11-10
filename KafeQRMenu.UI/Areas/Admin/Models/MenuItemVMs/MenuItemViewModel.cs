// MenuItemViewModel.cs (for Details view)
using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.Admin.Models.MenuItemVMs
{
    public class MenuItemViewModel
    {
        public Guid MenuItemId { get; set; }

        [Display(Name = "Ürün Adı")]
        public string MenuItemName { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Display(Name = "Sıralama")]
        public int SortOrder { get; set; }

        public Guid MenuCategoryId { get; set; }

        [Display(Name = "Kategori")]
        public string MenuCategoryName { get; set; }

        public Guid? ImageFileId { get; set; }

        public string? ImageBase64 { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime? UpdatedTime { get; set; }
    }
}