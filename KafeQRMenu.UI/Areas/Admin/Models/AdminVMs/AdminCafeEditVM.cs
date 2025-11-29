using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.Admin.Models.AdminVMs
{
    public class AdminCafeEditVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Kafe adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kafe adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kafe Adı")]
        public string CafeName { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        [Display(Name = "Adres")]
        public string? Address { get; set; }

        // Mevcut resim bilgisi
        public Guid? ImageFileId { get; set; }
        public byte[]? ImageFileBytes { get; set; }

        // Yeni resim yükleme
        [Display(Name = "Logo")]
        public IFormFile? ImageFile { get; set; }
    }
}