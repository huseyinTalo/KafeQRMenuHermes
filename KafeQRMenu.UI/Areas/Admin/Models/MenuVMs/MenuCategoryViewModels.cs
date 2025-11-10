using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.Admin.ViewModels.Menu
{
    /// <summary>
    /// ViewModel for Category Details page - showing category with its items
    /// </summary>
    public class CategoryDetailsViewModel
    {
        // Navigation context
        public Guid MenuId { get; set; }
        public string MenuName { get; set; }

        // Category data
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        public string CategoryName { get; set; }

        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public Guid CafeId { get; set; }
        public string CafeName { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }

        // Items in this category
        public List<ItemListItemViewModel> Items { get; set; } = new();

        // Navigation
        public string Breadcrumb => $"Menüler / {MenuName} / {CategoryName}";

        // Authorization flags
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;
        public bool CanManageItems { get; set; } = true;
        public bool CanCreateItem { get; set; } = true;

        // UI helpers
        public string ImageDataUrl => ImageFileBytes != null && ImageFileBytes.Length > 0
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(ImageFileBytes)}"
            : GeneratePlaceholderSvg();

        private string GeneratePlaceholderSvg()
        {
            var categoryNameEncoded = Uri.EscapeDataString(CategoryName ?? "Kategori");
            return $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23e7f5ff'/%3E%3Ctext x='50%25' y='45%25' font-family='Arial, sans-serif' font-size='64' fill='%23339af0' text-anchor='middle' dominant-baseline='middle'%3E%F0%9F%8F%B7%3C/text%3E%3Ctext x='50%25' y='65%25' font-family='Arial, sans-serif' font-size='18' fill='%2374c0fc' text-anchor='middle' dominant-baseline='middle'%3E{categoryNameEncoded}%3C/text%3E%3C/svg%3E";
        }

        public int TotalItems => Items.Count;
        public bool HasItems => Items.Any();
    }

    public class ItemListItemViewModel
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int SortOrder { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public DateTime CreatedTime { get; set; }

        // Authorization flags
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;

        // UI helpers
        public string ImageDataUrl => ImageFileBytes != null && ImageFileBytes.Length > 0
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(ImageFileBytes)}"
            : GeneratePlaceholderSvg();

        private string GeneratePlaceholderSvg()
        {
            var itemNameEncoded = Uri.EscapeDataString(ItemName ?? "Ürün");
            return $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23fff3e0'/%3E%3Ctext x='50%25' y='45%25' font-family='Arial, sans-serif' font-size='64' fill='%23ff9800' text-anchor='middle' dominant-baseline='middle'%3E%F0%9F%8D%BD%3C/text%3E%3Ctext x='50%25' y='65%25' font-family='Arial, sans-serif' font-size='18' fill='%23ffb74d' text-anchor='middle' dominant-baseline='middle'%3E{itemNameEncoded}%3C/text%3E%3C/svg%3E";
        }

        public string FormattedPrice => $"{Price:N2} ₺";
    }

    /// <summary>
    /// ViewModel for Category Create page
    /// </summary>
    public class CategoryCreateViewModel
    {
        // Navigation context
        public Guid MenuId { get; set; }
        public string MenuName { get; set; }
        public Guid CafeId { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Kategori Adı")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Range(0, 9999, ErrorMessage = "Sıra numarası 0-9999 arasında olmalıdır.")]
        [Display(Name = "Sıra")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Resim")]
        public IFormFile? ImageFile { get; set; }

        // Navigation
        public string Breadcrumb => $"Menüler / {MenuName} / Yeni Kategori";

        // Validation
        public List<string> ValidationErrors { get; set; } = new();

        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(CategoryName))
                ValidationErrors.Add("Kategori adı boş olamaz.");

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
        }

        public bool IsValid => ValidationErrors.Count == 0;
    }

    /// <summary>
    /// ViewModel for Category Edit page
    /// </summary>
    public class CategoryEditViewModel
    {
        // Navigation context
        public Guid MenuId { get; set; }
        public string MenuName { get; set; }

        // Category data
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Kategori Adı")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Range(0, 9999, ErrorMessage = "Sıra numarası 0-9999 arasında olmalıdır.")]
        [Display(Name = "Sıra")]
        public int SortOrder { get; set; }

        public Guid CafeId { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[]? CurrentImageBytes { get; set; }

        [Display(Name = "Yeni Resim")]
        public IFormFile? NewImageFile { get; set; }

        [Display(Name = "Mevcut Resmi Sil")]
        public bool RemoveCurrentImage { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }

        // UI helpers
        public string CurrentImageDataUrl => CurrentImageBytes != null && CurrentImageBytes.Length > 0
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(CurrentImageBytes)}"
            : GeneratePlaceholderSvg();

        private string GeneratePlaceholderSvg()
        {
            var categoryNameEncoded = Uri.EscapeDataString(CategoryName ?? "Kategori");
            return $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23e7f5ff'/%3E%3Ctext x='50%25' y='45%25' font-family='Arial, sans-serif' font-size='64' fill='%23339af0' text-anchor='middle' dominant-baseline='middle'%3E%F0%9F%8F%B7%3C/text%3E%3Ctext x='50%25' y='65%25' font-family='Arial, sans-serif' font-size='18' fill='%2374c0fc' text-anchor='middle' dominant-baseline='middle'%3E{categoryNameEncoded}%3C/text%3E%3C/svg%3E";
        }

        public bool HasCurrentImage => CurrentImageBytes != null && CurrentImageBytes.Length > 0;
        public bool WillUploadNewImage => NewImageFile != null && NewImageFile.Length > 0;

        // Navigation
        public string Breadcrumb => $"Menüler / {MenuName} / {CategoryName} / Düzenle";

        // Validation
        public List<string> ValidationErrors { get; set; } = new();

        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(CategoryName))
                ValidationErrors.Add("Kategori adı boş olamaz.");

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
        }

        public bool IsValid => ValidationErrors.Count == 0;
    }
}