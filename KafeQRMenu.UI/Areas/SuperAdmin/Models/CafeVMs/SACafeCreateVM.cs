using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.SuperAdmin.Models.CafeVMs
{
    public class SACafeCreateVM
    {
        [Required(ErrorMessage = "Cafe adı gereklidir.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Cafe adı 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Cafe Adı")]
        public string CafeName { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir.")]
        [Display(Name = "Adres")]
        public string Address { get; set; }
        public string DomainName { get; set; }
    }
}
