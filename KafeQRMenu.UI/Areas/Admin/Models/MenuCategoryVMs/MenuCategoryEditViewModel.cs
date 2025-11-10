using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.Admin.Models.MenuCategoryVMs
{
    public class MenuCategoryEditViewModel
    {
        [HiddenInput]
        public Guid MenuCategoryId { get; set; }

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
        public int SortOrder { get; set; }

        [HiddenInput]
        public Guid CafeId { get; set; }

        [HiddenInput]
        public Guid? ImageFileId { get; set; }

        [Display(Name = "Yeni Resim Yükle")]
        public IFormFile? ImageFile { get; set; }

        // Used to display current image in the view
        public byte[]? ImageFileBytes { get; set; }
    }
}