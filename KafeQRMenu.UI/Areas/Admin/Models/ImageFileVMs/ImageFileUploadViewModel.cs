using KafeQRMenu.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.Admin.Models.ImageFileVMs
{
    public class ImageFileUploadViewModel
    {
        [Required(ErrorMessage = "Resim dosyası zorunludur")]
        [Display(Name = "Resim Dosyası")]
        public IFormFile ImageFile { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Resim tipi seçimi zorunludur")]
        [Display(Name = "Resim Tipi")]
        public ImageContentType ImageContentType { get; set; }

        [Display(Name = "İlişkili Kayıt")]
        public Guid? RelatedEntityId { get; set; }

        // For display
        public string? PreviewUrl { get; set; }
    }
}
