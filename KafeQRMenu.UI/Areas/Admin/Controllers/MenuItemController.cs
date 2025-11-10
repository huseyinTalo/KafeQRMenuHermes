using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using KafeQRMenu.UI.Areas.Admin.Models.MenuItemVMs;
using KafeQRMenu.BLogic.Services.ImageServices;

namespace KafeQRMenu.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuItemController : Controller
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly IImageFileService _imageFileService;
        private readonly ILogger<MenuItemController> _logger;

        public MenuItemController(
            IMenuItemService menuItemService,
            IMenuCategoryService menuCategoryService,
            IImageFileService imageFileService,
            ILogger<MenuItemController> logger)
        {
            _menuItemService = menuItemService;
            _menuCategoryService = menuCategoryService;
            _imageFileService = imageFileService;
            _logger = logger;
        }

        // GET: Admin/MenuItem
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cafeIdClaim = User.FindFirst("CafeId")?.Value;
                if (!Guid.TryParse(cafeIdClaim, out Guid cafeId))
                {
                    TempData["ErrorMessage"] = "Kafe bilgisi alınamadı.";
                    return View(new List<MenuItemListDTO>());
                }

                var result = await _menuItemService.GetAllAsyncCafesCatsItems(cafeId);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(new List<MenuItemListDTO>());
                }

                return View(result.Data ?? new List<MenuItemListDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün listesi yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Ürünler yüklenirken bir hata oluştu.";
                return View(new List<MenuItemListDTO>());
            }
        }

        // GET: Admin/MenuItem/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz ürün ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _menuItemService.GetByIdAsync(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new MenuItemViewModel
                {
                    MenuItemId = result.Data.MenuItemId,
                    MenuItemName = result.Data.MenuItemName,
                    Description = result.Data.Description,
                    Price = result.Data.Price,
                    SortOrder = result.Data.SortOrder,
                    MenuCategoryId = result.Data.MenuCategoryId,
                    MenuCategoryName = result.Data.MenuCategoryName,
                    ImageFileId = result.Data.ImageFileId,
                    CreatedTime = result.Data.CreatedTime,
                    UpdatedTime = result.Data.UpdatedTime
                };

                // Load image if exists
                if (result.Data.ImageFileBytes != null && result.Data.ImageFileBytes.Length > 0)
                {
                    viewModel.ImageBase64 = Convert.ToBase64String(result.Data.ImageFileBytes);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün detayı yüklenirken hata: {Id}", id);
                TempData["ErrorMessage"] = "Ürün detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/MenuItem/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategories();

            var model = new MenuItemCreateViewModel();
            return View(model);
        }

        // POST: Admin/MenuItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItemCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                TempData["WarningMessage"] = "Lütfen tüm alanları kontrol edin.";
                return View(model);
            }

            try
            {
                byte[]? imageData = null;

                // Process image file if provided
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var dto = new MenuItemCreateDTO
                {
                    MenuItemName = model.MenuItemName,
                    Description = model.Description,
                    Price = model.Price,
                    SortOrder = model.SortOrder,
                    MenuCategoryId = model.MenuCategoryId
                };

                var result = await _menuItemService.CreateAsync(dto, imageData);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.Message;
                await LoadCategories();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün eklenirken hata oluştu");
                TempData["ErrorMessage"] = "Ürün eklenirken bir hata oluştu.";
                await LoadCategories();
                return View(model);
            }
        }

        // GET: Admin/MenuItem/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz ürün ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _menuItemService.GetByIdAsync(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                var model = new MenuItemEditViewModel
                {
                    MenuItemId = result.Data.MenuItemId,
                    MenuItemName = result.Data.MenuItemName,
                    Description = result.Data.Description,
                    Price = result.Data.Price,
                    SortOrder = result.Data.SortOrder,
                    MenuCategoryId = result.Data.MenuCategoryId,
                    ImageFileId = result.Data.ImageFileId
                };

                // Load current image if exists
                if (result.Data.ImageFileBytes != null && result.Data.ImageFileBytes.Length > 0)
                {
                    model.CurrentImageBase64 = Convert.ToBase64String(result.Data.ImageFileBytes);
                }

                await LoadCategories();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün düzenleme formu yüklenirken hata: {Id}", id);
                TempData["ErrorMessage"] = "Ürün bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/MenuItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MenuItemEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                TempData["WarningMessage"] = "Lütfen tüm alanları kontrol edin.";
                return View(model);
            }

            try
            {
                byte[]? newImageData = null;

                // Process new image file if provided
                if (model.NewImageFile != null && model.NewImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.NewImageFile.CopyToAsync(memoryStream);
                        newImageData = memoryStream.ToArray();
                    }
                }

                var dto = new MenuItemUpdateDTO
                {
                    MenuItemId = model.MenuItemId,
                    MenuItemName = model.MenuItemName,
                    Description = model.Description,
                    Price = model.Price,
                    SortOrder = model.SortOrder,
                    MenuCategoryId = model.MenuCategoryId,
                };

                var result = await _menuItemService.UpdateAsync(dto, newImageData);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.Message;
                await LoadCategories();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün güncellenirken hata: {Id}", model.MenuItemId);
                TempData["ErrorMessage"] = "Ürün güncellenirken bir hata oluştu.";
                await LoadCategories();
                return View(model);
            }
        }

        // GET: Admin/MenuItem/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz ürün ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _menuItemService.GetByIdAsync(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Silme onayı yüklenirken hata: {Id}", id);
                TempData["ErrorMessage"] = "Ürün bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/MenuItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var dto = new MenuItemDTO { MenuItemId = id };
                var result = await _menuItemService.DeleteAsync(dto);

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
                _logger.LogError(ex, "Ürün silinirken hata: {Id}", id);
                TempData["ErrorMessage"] = "Ürün silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // API endpoint for menu item count
        [HttpGet]
        public async Task<IActionResult> GetMenuItemCount()
        {
            try
            {
                var cafeIdClaim = User.FindFirst("CafeId")?.Value;
                if (!Guid.TryParse(cafeIdClaim, out Guid cafeId))
                {
                    return Json(0);
                }

                var result = await _menuItemService.GetAllAsyncCafesCatsItems(cafeId);
                var count = result.IsSuccess ? result.Data?.Count ?? 0 : 0;
                return Json(count);
            }
            catch
            {
                return Json(0);
            }
        }

        // Helper method to load categories
        private async Task LoadCategories()
        {
            var cafeIdClaim = User.FindFirst("CafeId")?.Value;
            if (Guid.TryParse(cafeIdClaim, out Guid cafeId))
            {
                var categoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(cafeId);

                if (categoriesResult.IsSuccess && categoriesResult.Data != null)
                {
                    ViewBag.Categories = new SelectList(
                        categoriesResult.Data.OrderBy(c => c.SortOrder),
                        "MenuCategoryId",
                        "MenuCategoryName"
                    );
                    return;
                }
            }

            ViewBag.Categories = new SelectList(new List<MenuCategoryListDTO>(), "MenuCategoryId", "MenuCategoryName");
        }
    }
}