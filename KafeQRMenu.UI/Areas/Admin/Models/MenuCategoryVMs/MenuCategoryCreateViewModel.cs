using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.Admin.Models.MenuCategoryVMs
{
    public class MenuCategoryCreateViewModel
    {
        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı 2-100 karakter arasında olmalıdır")]
        [Display(Name = "Kategori Adı")]
        public string MenuCategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama maksimum 500 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sıralama zorunludur")]
        [Range(1, 1000, ErrorMessage = "Sıralama 1-1000 arasında olmalıdır")]
        [Display(Name = "Sıralama")]
        public int SortOrder { get; set; } = 1;

        [HiddenInput]
        public Guid CafeId { get; set; }

        [Display(Name = "Kategori Resmi")]
        public Guid? ImageFileId { get; set; }

        [Display(Name = "Resim Yükle")]
        public IFormFile? ImageFile { get; set; }
    }
}