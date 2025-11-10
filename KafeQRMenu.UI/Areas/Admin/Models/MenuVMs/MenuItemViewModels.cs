using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.Admin.ViewModels.Menu
{
    /// <summary>
    /// ViewModel for MenuItem Create page
    /// </summary>
    public class ItemCreateViewModel
    {
        // Navigation context
        public Guid MenuId { get; set; }
        public string MenuName { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ürün adı 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Ürün Adı")]
        public string ItemName { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir.")]
        [Range(0.01, 999999.99, ErrorMessage = "Fiyat 0.01 ile 999999.99 arasında olmalıdır.")]
        [Display(Name = "Fiyat (₺)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; } = 0;

        [Range(0, 9999, ErrorMessage = "Sıra numarası 0-9999 arasında olmalıdır.")]
        [Display(Name = "Sıra")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Resim")]
        public IFormFile? ImageFile { get; set; }

        // Navigation
        public string Breadcrumb => $"Menüler / {MenuName} / {CategoryName} / Yeni Ürün";

        // Validation
        public List<string> ValidationErrors { get; set; } = new();

        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(ItemName))
                ValidationErrors.Add("Ürün adı boş olamaz.");

            if (Price <= 0)
                ValidationErrors.Add("Fiyat sıfırdan büyük olmalıdır.");

            if (Price > 999999.99m)
                ValidationErrors.Add("Fiyat çok yüksek.");

            if (SortOrder < 0)
                ValidationErrors.Add("Sıra numarası negatif olamaz.");

            if (ImageFile != null && ImageFile.Length > 5 * 1024 * 1024) // 5MB
                ValidationErrors.Add("Resim boyutu 5MB'dan küçük olmalıdır.");

            if (ImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    ValidationErrors.Add("Sadece resim dosyaları yüklenebilir (jpg, jpeg, png, gif, webp).");
            }

            // Business rule: Items with expensive prices should have descriptions
            if (Price > 100 && string.IsNullOrWhiteSpace(Description))
                ValidationErrors.Add("100 TL üzeri ürünler için açıklama önerilir.");
        }

        public bool IsValid => ValidationErrors.Count == 0;
        public bool HasWarnings => ValidationErrors.Any(e => e.Contains("önerilir"));
    }

    /// <summary>
    /// ViewModel for MenuItem Edit page
    /// </summary>
    public class ItemEditViewModel
    {
        // Navigation context
        public Guid MenuId { get; set; }
        public string MenuName { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Item data
        public Guid ItemId { get; set; }

        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ürün adı 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Ürün Adı")]
        public string ItemName { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir.")]
        [Range(0.01, 999999.99, ErrorMessage = "Fiyat 0.01 ile 999999.99 arasında olmalıdır.")]
        [Display(Name = "Fiyat (₺)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Range(0, 9999, ErrorMessage = "Sıra numarası 0-9999 arasında olmalıdır.")]
        [Display(Name = "Sıra")]
        public int SortOrder { get; set; }

        public Guid? ImageFileId { get; set; }
        public byte[]? CurrentImageBytes { get; set; }

        [Display(Name = "Yeni Resim")]
        public IFormFile? NewImageFile { get; set; }

        [Display(Name = "Mevcut Resmi Sil")]
        public bool RemoveCurrentImage { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }

        // For tracking price changes (useful for business logic)
        public decimal? OriginalPrice { get; set; }
        public bool IsPriceChanged => OriginalPrice.HasValue && OriginalPrice.Value != Price;
        public decimal PriceChangeAmount => OriginalPrice.HasValue ? Price - OriginalPrice.Value : 0;
        public decimal PriceChangePercentage => OriginalPrice.HasValue && OriginalPrice.Value > 0
            ? ((Price - OriginalPrice.Value) / OriginalPrice.Value) * 100
            : 0;

        // UI helpers
        public string CurrentImageDataUrl => CurrentImageBytes != null && CurrentImageBytes.Length > 0
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(CurrentImageBytes)}"
            : GeneratePlaceholderSvg();

        private string GeneratePlaceholderSvg()
        {
            var itemNameEncoded = Uri.EscapeDataString(ItemName ?? "Ürün");
            return $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23fff3e0'/%3E%3Ctext x='50%25' y='45%25' font-family='Arial, sans-serif' font-size='64' fill='%23ff9800' text-anchor='middle' dominant-baseline='middle'%3E%F0%9F%8D%BD%3C/text%3E%3Ctext x='50%25' y='65%25' font-family='Arial, sans-serif' font-size='18' fill='%23ffb74d' text-anchor='middle' dominant-baseline='middle'%3E{itemNameEncoded}%3C/text%3E%3C/svg%3E";
        }

        public bool HasCurrentImage => CurrentImageBytes != null && CurrentImageBytes.Length > 0;
        public bool WillUploadNewImage => NewImageFile != null && NewImageFile.Length > 0;

        public string FormattedPrice => $"{Price:N2} ₺";
        public string FormattedOriginalPrice => OriginalPrice.HasValue ? $"{OriginalPrice.Value:N2} ₺" : "";

        // Navigation
        public string Breadcrumb => $"Menüler / {MenuName} / {CategoryName} / {ItemName} / Düzenle";

        // Validation
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> ValidationWarnings { get; set; } = new();

        public void Validate()
        {
            ValidationErrors.Clear();
            ValidationWarnings.Clear();

            if (string.IsNullOrWhiteSpace(ItemName))
                ValidationErrors.Add("Ürün adı boş olamaz.");

            if (Price <= 0)
                ValidationErrors.Add("Fiyat sıfırdan büyük olmalıdır.");

            if (Price > 999999.99m)
                ValidationErrors.Add("Fiyat çok yüksek.");

            if (SortOrder < 0)
                ValidationErrors.Add("Sıra numarası negatif olamaz.");

            if (NewImageFile != null && NewImageFile.Length > 5 * 1024 * 1024) // 5MB
                ValidationErrors.Add("Resim boyutu 5MB'dan küçük olmalıdır.");

            if (NewImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(NewImageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    ValidationErrors.Add("Sadece resim dosyaları yüklenebilir (jpg, jpeg, png, gif, webp).");
            }

            // Business rule warnings
            if (Price > 100 && string.IsNullOrWhiteSpace(Description))
                ValidationWarnings.Add("100 TL üzeri ürünler için açıklama önerilir.");

            // Price change warnings
            if (IsPriceChanged && Math.Abs(PriceChangePercentage) > 50)
                ValidationWarnings.Add($"Fiyat %{Math.Abs(PriceChangePercentage):N0} değişti. Bu kadar büyük bir değişiklik yapmak istediğinizden emin misiniz?");

            if (IsPriceChanged && PriceChangeAmount < 0 && Math.Abs(PriceChangePercentage) > 30)
                ValidationWarnings.Add("Fiyat %30'dan fazla düştü. Müşteriler için indirim/kampanya duyurusu yapmayı düşünün.");

            // Image warnings
            if (!HasCurrentImage && NewImageFile == null)
                ValidationWarnings.Add("Ürün görseli eklemeniz önerilir.");
        }

        public bool IsValid => ValidationErrors.Count == 0;
        public bool HasWarnings => ValidationWarnings.Count > 0;
    }
}