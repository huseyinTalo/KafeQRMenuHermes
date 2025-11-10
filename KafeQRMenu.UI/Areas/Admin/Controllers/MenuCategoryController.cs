using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.UI.Areas.Admin.Models.MenuCategoryVMs;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KafeQRMenu.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuCategoryController : Controller
    {
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly ILogger<MenuCategoryController> _logger;

        public MenuCategoryController(
            IMenuCategoryService menuCategoryService,
            ILogger<MenuCategoryController> logger)
        {
            _menuCategoryService = menuCategoryService;
            _logger = logger;
        }

        // GET: Admin/MenuCategory
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cafeIdClaim = User.FindFirst("CafeId")?.Value;
                Guid.TryParse(cafeIdClaim, out Guid CafeId);
                var result = await _menuCategoryService.GetAllAsyncCafesCats(CafeId);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(new List<MenuCategoryListViewModel>());
                }

                return View(result.Data.Adapt<List<MenuCategoryListViewModel>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori listesi yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Kategoriler yüklenirken bir hata oluştu.";
                return View(new List<MenuCategoryListViewModel>());
            }
        }

        // GET: Admin/MenuCategory/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz kategori ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _menuCategoryService.GetByIdAsync(id);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                return View(result.Data.Adapt<MenuCategoryItemViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori detayı yüklenirken hata: {Id}", id);
                TempData["ErrorMessage"] = "Kategori detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/MenuCategory/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new MenuCategoryCreateViewModel
            {
                CafeId = GetCurrentCafeId(),
                SortOrder = 1
            };

            return View(model);
        }

        // POST: Admin/MenuCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuCategoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["WarningMessage"] = "Lütfen tüm alanları kontrol edin.";
                return View(model);
            }

            try
            {
                byte[] imageData = null;

                // Process uploaded image if exists
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Validate image file
                    var validationError = ValidateImage(model.ImageFile);
                    if (validationError != null)
                    {
                        TempData["ErrorMessage"] = validationError;
                        return View(model);
                    }

                    // Read image data
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var dto = new MenuCategoryCreateDTO
                {
                    MenuCategoryName = model.MenuCategoryName,
                    Description = model.Description,
                    SortOrder = model.SortOrder,
                    CafeId = GetCurrentCafeId()
                };

                // Service handles image creation within transaction
                var result = await _menuCategoryService.CreateAsync(dto, imageData);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori eklenirken hata oluştu");
                TempData["ErrorMessage"] = "Kategori eklenirken bir hata oluştu.";
                return View(model);
            }
        }

        // GET: Admin/MenuCategory/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz kategori ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _menuCategoryService.GetByIdAsync(id);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                var model = new MenuCategoryEditViewModel
                {
                    MenuCategoryId = result.Data.MenuCategoryId,
                    MenuCategoryName = result.Data.MenuCategoryName,
                    Description = result.Data.Description,
                    SortOrder = result.Data.SortOrder,
                    CafeId = result.Data.CafeId,
                    ImageFileId = result.Data.ImageFileId,
                    ImageFileBytes = result.Data.ImageFileBytes
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori düzenleme formu yüklenirken hata: {Id}", id);
                TempData["ErrorMessage"] = "Kategori bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/MenuCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MenuCategoryEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["WarningMessage"] = "Lütfen tüm alanları kontrol edin.";
                return View(model);
            }

            try
            {
                byte[] newImageData = null;

                // Process new uploaded image if exists
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Validate image file
                    var validationError = ValidateImage(model.ImageFile);
                    if (validationError != null)
                    {
                        TempData["ErrorMessage"] = validationError;
                        return View(model);
                    }

                    // Read new image data
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImageFile.CopyToAsync(memoryStream);
                        newImageData = memoryStream.ToArray();
                    }
                }

                var dto = new MenuCategoryUpdateDTO
                {
                    MenuCategoryId = model.MenuCategoryId,
                    MenuCategoryName = model.MenuCategoryName,
                    Description = model.Description,
                    SortOrder = model.SortOrder,
                    CafeId = model.CafeId,
                    ImageFileId = model.ImageFileId,
                    
                };

                // Service handles image update/replacement within transaction
                var result = await _menuCategoryService.UpdateAsync(dto, newImageData);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori güncellenirken hata: {Id}", model.MenuCategoryId);
                TempData["ErrorMessage"] = "Kategori güncellenirken bir hata oluştu.";
                return View(model);
            }
        }

        // GET: Admin/MenuCategory/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz kategori ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _menuCategoryService.GetByIdAsync(id);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                return View(result.Data.Adapt<MenuCategoryItemViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Silme onayı yüklenirken hata: {Id}", id);
                TempData["ErrorMessage"] = "Kategori bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/MenuCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var dto = new MenuCategoryDTO { MenuCategoryId = id };

                // Service handles both category and image deletion within transaction
                var result = await _menuCategoryService.DeleteAsync(dto);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori silinirken hata: {Id}", id);
                TempData["ErrorMessage"] = "Kategori silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // API endpoints for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetCategoryCount()
        {
            try
            {
                var result = await _menuCategoryService.GetAllAsync();
                var count = result.IsSuccess ? result.Data?.Count ?? 0 : 0;
                return Json(count);
            }
            catch
            {
                return Json(0);
            }
        }

        // Helper methods
        private Guid GetCurrentCafeId()
        {
            var cafeIdClaim = User.FindFirst("CafeId")?.Value;
            if (Guid.TryParse(cafeIdClaim, out Guid cafeId))
            {
                return cafeId;
            }

            return Guid.Empty;
        }

        private string ValidateImage(IFormFile file)
        {
            if (file == null)
                return "Resim dosyası seçilmedi.";

            // Consistent validation for all image operations
            var allowedExtensions = new[] { ".jpg", ".jpeg"};
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return "Geçersiz resim formatı. Lütfen JPG formatında bir resim yükleyin.";

            var allowedContentTypes = new[] { "image/jpeg" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return "Geçersiz resim içerik tipi.";

            // Check file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return "Resim boyutu 5MB'dan küçük olmalıdır.";

            if (file.Length == 0)
                return "Resim dosyası boş olamaz.";

            return null; // Valid
        }
    }
}