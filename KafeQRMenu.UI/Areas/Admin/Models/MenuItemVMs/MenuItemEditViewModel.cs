// MenuItemEditViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KafeQRMenu.UI.Areas.Admin.Models.MenuItemVMs
{
    public class MenuItemEditViewModel
    {
        public Guid MenuItemId { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        [Display(Name = "Ürün Adı")]
        public string MenuItemName { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, 999999.99, ErrorMessage = "Fiyat 0.01 ile 999999.99 arasında olmalıdır")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Sıralama zorunludur")]
        [Range(1, 1000, ErrorMessage = "Sıralama 1 ile 1000 arasında olmalıdır")]
        [Display(Name = "Sıralama")]
        public int SortOrder { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        [Display(Name = "Kategori")]
        public Guid MenuCategoryId { get; set; }

        [Display(Name = "Yeni Resim")]
        public IFormFile? NewImageFile { get; set; }

        public Guid? ImageFileId { get; set; }

        public string? CurrentImageBase64 { get; set; }
    }
}