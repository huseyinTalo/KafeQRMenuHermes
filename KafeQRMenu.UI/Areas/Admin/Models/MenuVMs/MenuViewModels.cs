using KafeQRMenu.BLogic.DTOs.MenuDTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KafeQRMenu.UI.Areas.Admin.ViewModels.Menu
{
    /// <summary>
    /// ViewModel for Menu Index page - listing all menus
    /// </summary>
    public class MenuIndexViewModel
    {
        public List<MenuListItemViewModel> Menus { get; set; } = new();
        public bool CanCreate { get; set; } = true;
        public int TotalCount { get; set; }
    }

    public class MenuListItemViewModel
    {
        public Guid MenuId { get; set; }
        public string MenuName { get; set; }
        public bool IsActive { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public Guid CafeId { get; set; }
        public string CafeName { get; set; }
        public int CategoryCount { get; set; }
        public DateTime CreatedTime { get; set; }

        // Authorization flags
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;
        public bool CanViewDetails { get; set; } = true;
        public bool CanToggleActive { get; set; } = true;

        // UI helpers
        public string ImageDataUrl => ImageFileBytes != null && ImageFileBytes.Length > 0
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(ImageFileBytes)}"
            : GeneratePlaceholderSvg();

        private string GeneratePlaceholderSvg()
        {
            var menuNameEncoded = Uri.EscapeDataString(MenuName ?? "Menü");
            return $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23f8f9fa'/%3E%3Ctext x='50%25' y='45%25' font-family='Arial, sans-serif' font-size='64' fill='%23dee2e6' text-anchor='middle' dominant-baseline='middle'%3E%F0%9F%93%96%3C/text%3E%3Ctext x='50%25' y='65%25' font-family='Arial, sans-serif' font-size='18' fill='%23adb5bd' text-anchor='middle' dominant-baseline='middle'%3E{menuNameEncoded}%3C/text%3E%3C/svg%3E";
        }
    }

    /// <summary>
    /// ViewModel for Menu Details page - showing menu with its categories
    /// </summary>
    public class MenuDetailsViewModel
    {
        public Guid MenuId { get; set; }

        [Required(ErrorMessage = "Menü adı gereklidir.")]
        [StringLength(100, ErrorMessage = "Menü adı en fazla 100 karakter olabilir.")]
        public string MenuName { get; set; }

        public bool IsActive { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public Guid CafeId { get; set; }
        public string CafeName { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }

        // Categories for this menu
        public List<CategoryListItemViewModel> AssignedCategories { get; set; } = new();
        public List<CategoryListItemViewModel> AvailableCategories { get; set; } = new();

        // Navigation
        public string Breadcrumb => $"Menüler / {MenuName}";

        // Authorization flags
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;
        public bool CanManageCategories { get; set; } = true;
        public bool CanCreateCategory { get; set; } = true;

        // UI helpers
        public string ImageDataUrl => ImageFileBytes != null && ImageFileBytes.Length > 0
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(ImageFileBytes)}"
            : GeneratePlaceholderSvg();

        private string GeneratePlaceholderSvg()
        {
            var menuNameEncoded = Uri.EscapeDataString(MenuName ?? "Menü");
            return $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23f8f9fa'/%3E%3Ctext x='50%25' y='45%25' font-family='Arial, sans-serif' font-size='64' fill='%23dee2e6' text-anchor='middle' dominant-baseline='middle'%3E%F0%9F%93%96%3C/text%3E%3Ctext x='50%25' y='65%25' font-family='Arial, sans-serif' font-size='18' fill='%23adb5bd' text-anchor='middle' dominant-baseline='middle'%3E{menuNameEncoded}%3C/text%3E%3C/svg%3E";
        }
    }

    public class CategoryListItemViewModel
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public Guid? ImageFileId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public int ItemCount { get; set; }
        public bool IsAssignedToMenu { get; set; }

        // Authorization flags
        public bool CanEdit { get; set; } = true;
        public bool CanDelete { get; set; } = true;
        public bool CanViewDetails { get; set; } = true;

        // UI helpers
        public string ImageDataUrl => ImageFileBytes != null && ImageFileBytes.Length > 0
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(ImageFileBytes)}"
            : GeneratePlaceholderSvg();

        private string GeneratePlaceholderSvg()
        {
            var categoryNameEncoded = Uri.EscapeDataString(CategoryName ?? "Kategori");
            return $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23e7f5ff'/%3E%3Ctext x='50%25' y='45%25' font-family='Arial, sans-serif' font-size='64' fill='%23339af0' text-anchor='middle' dominant-baseline='middle'%3E%F0%9F%8F%B7%3C/text%3E%3Ctext x='50%25' y='65%25' font-family='Arial, sans-serif' font-size='18' fill='%2374c0fc' text-anchor='middle' dominant-baseline='middle'%3E{categoryNameEncoded}%3C/text%3E%3C/svg%3E";
        }
    }

    /// <summary>
    /// ViewModel for Menu Create page
    /// </summary>
    public class MenuCreateViewModel
    {
        [Required(ErrorMessage = "Menü adı gereklidir.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Menü adı 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Menü Adı")]
        public string MenuName { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Kafe seçimi gereklidir.")]
        [Display(Name = "Kafe")]
        public Guid CafeId { get; set; }

        [Display(Name = "Kategoriler")]
        public List<Guid>? CategoryIds { get; set; }

        [Display(Name = "Resim")]
        public IFormFile? ImageFile { get; set; }

        public MultiSelectList? Categories { get; set; }

        // Validation
        public bool HasValidCafe => CafeId != Guid.Empty;
        public bool HasImage => ImageFile != null && ImageFile.Length > 0;

        // Business rules validation
        public List<string> ValidationErrors { get; set; } = new();

        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(MenuName))
                ValidationErrors.Add("Menü adı boş olamaz.");

            if (CafeId == Guid.Empty)
                ValidationErrors.Add("Bir kafe seçmelisiniz.");

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
    /// ViewModel for Menu Edit page
    /// </summary>
    public class MenuEditViewModel
    {
        public Guid MenuId { get; set; }

        [Required(ErrorMessage = "Menü adı gereklidir.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Menü adı 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Menü Adı")]
        public string MenuName { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Kafe seçimi gereklidir.")]
        [Display(Name = "Kafe")]
        public Guid CafeId { get; set; }

        [Display(Name = "Kategoriler")]
        public List<Guid>? CategoryIds { get; set; }

        public Guid? ImageFileId { get; set; }
        public byte[]? CurrentImageBytes { get; set; }

        [Display(Name = "Yeni Resim")]
        public IFormFile? NewImageFile { get; set; }

        [Display(Name = "Mevcut Resmi Sil")]
        public bool RemoveCurrentImage { get; set; }

        public MultiSelectList? Categories { get; set; }

        // UI helpers
        public string CurrentImageDataUrl => CurrentImageBytes != null && CurrentImageBytes.Length > 0
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(CurrentImageBytes)}"
            : GeneratePlaceholderSvg();

        private string GeneratePlaceholderSvg()
        {
            var menuNameEncoded = Uri.EscapeDataString(MenuName ?? "Menü");
            return $"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23f8f9fa'/%3E%3Ctext x='50%25' y='45%25' font-family='Arial, sans-serif' font-size='64' fill='%23dee2e6' text-anchor='middle' dominant-baseline='middle'%3E%F0%9F%93%96%3C/text%3E%3Ctext x='50%25' y='65%25' font-family='Arial, sans-serif' font-size='18' fill='%23adb5bd' text-anchor='middle' dominant-baseline='middle'%3E{menuNameEncoded}%3C/text%3E%3C/svg%3E";
        }

        public bool HasCurrentImage => CurrentImageBytes != null && CurrentImageBytes.Length > 0;
        public bool WillUploadNewImage => NewImageFile != null && NewImageFile.Length > 0;

        // Navigation
        public string Breadcrumb => $"Menüler / {MenuName} / Düzenle";

        // Business rules validation
        public List<string> ValidationErrors { get; set; } = new();

        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(MenuName))
                ValidationErrors.Add("Menü adı boş olamaz.");

            if (CafeId == Guid.Empty)
                ValidationErrors.Add("Bir kafe seçmelisiniz.");

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