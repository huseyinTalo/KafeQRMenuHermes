using KafeQRMenu.BLogic.DTOs.CafeDTOs;
using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.DTOs.MenuDTOs;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.BLogic.Services.MenuService;
using KafeQRMenu.UI.Areas.Admin.ViewModels.Menu;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KafeQRMenu.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly IMenuItemService _menuItemService;
        private readonly ILogger<MenuController> _logger;

        public MenuController(
            IMenuService menuService,
            IMenuCategoryService menuCategoryService,
            IMenuItemService menuItemService,
            ILogger<MenuController> logger)
        {
            _menuService = menuService;
            _menuCategoryService = menuCategoryService;
            _menuItemService = menuItemService;
            _logger = logger;
        }

        #region Helper Methods for Claims

        /// <summary>
        /// Gets the CafeId from the current user's claims
        /// </summary>
        private Guid GetUserCafeId()
        {
            var cafeIdClaim = User.FindFirst("CafeId")?.Value;

            if (string.IsNullOrEmpty(cafeIdClaim))
            {
                _logger.LogWarning("CafeId claim not found for user: {UserId}", User.Identity?.Name);
                return Guid.Empty;
            }

            if (Guid.TryParse(cafeIdClaim, out var cafeId))
            {
                return cafeId;
            }

            _logger.LogError("Invalid CafeId claim format for user: {UserId}, Value: {CafeId}",
                User.Identity?.Name, cafeIdClaim);
            return Guid.Empty;
        }

        /// <summary>
        /// Gets the CafeName from the current user's claims
        /// </summary>
        private string GetUserCafeName()
        {
            var cafeName = User.FindFirst("CafeName")?.Value;

            if (string.IsNullOrEmpty(cafeName))
            {
                _logger.LogWarning("CafeName claim not found for user: {UserId}", User.Identity?.Name);
                return "Kafe";
            }

            return cafeName;
        }

        /// <summary>
        /// Validates that the user has a valid CafeId claim and returns both CafeId and CafeName
        /// </summary>
        private bool ValidateUserCafeId(out Guid cafeId, out string cafeName)
        {
            cafeId = GetUserCafeId();
            cafeName = GetUserCafeName();

            if (cafeId == Guid.Empty)
            {
                TempData["Error"] = "Kullanıcı kafe bilgisi bulunamadı. Lütfen yeniden giriş yapın.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Overload for when only CafeId is needed
        /// </summary>
        private bool ValidateUserCafeId(out Guid cafeId)
        {
            return ValidateUserCafeId(out cafeId, out _);
        }

        #endregion

        #region Menu CRUD

        // GET: Admin/Menu/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!ValidateUserCafeId(out var cafeId, out var cafeName))
                {
                    return View(new MenuIndexViewModel());
                }
                var userClaim = User.FindFirst("CafeId")?.Value;
                Guid.TryParse(userClaim, out Guid CafeId);
                var result = await _menuService.GetAllAsyncCafesCatsItems(CafeId);

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Menüler yüklenirken bir hata oluştu.";
                    return View(new MenuIndexViewModel());
                }


                // Use Mapster for mapping
                var menuViewModels = result.Data.Adapt<List<MenuListItemViewModel>>();

                var viewModel = new MenuIndexViewModel
                {
                    Menus = menuViewModels,
                    CanCreate = true,
                    TotalCount = menuViewModels.Count
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Menu Index");
                TempData["Error"] = "Menüler yüklenirken bir hata oluştu.";
                return View(new MenuIndexViewModel());
            }
        }

        // GET: Admin/Menu/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz menü ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ValidateUserCafeId(out var userCafeId, out var cafeName))
                {
                    return RedirectToAction(nameof(Index));
                }

                var menuResult = await _menuService.GetByIdAsync(id);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = menuResult.Message;
                    return RedirectToAction(nameof(Index));
                }

                // Security check: Ensure menu belongs to user's cafe
                if (menuResult.Data.CafeId != userCafeId)
                {
                    _logger.LogWarning("User attempted to view menu from different cafe. UserId: {UserId}, MenuCafeId: {MenuCafeId}, UserCafeId: {UserCafeId}",
                        User.Identity?.Name, menuResult.Data.CafeId, userCafeId);
                    TempData["Error"] = "Bu menüyü görüntüleme yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                // Get categories for this menu's cafe
                var categoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(menuResult.Data.CafeId);

                // Map assigned categories
                var assignedCategories = menuResult.Data.Categories?
                    .Adapt<List<CategoryListItemViewModel>>() ?? new List<CategoryListItemViewModel>();

                assignedCategories.ForEach(c => c.IsAssignedToMenu = true);

                // Map available categories
                var availableCategories = categoriesResult.Data?
                    .Where(c => !(menuResult.Data.CategoryIds?.Contains(c.MenuCategoryId) ?? false))
                    .Adapt<List<CategoryListItemViewModel>>() ?? new List<CategoryListItemViewModel>();

                availableCategories.ForEach(c => c.IsAssignedToMenu = false);

                // Map to ViewModel
                var viewModel = new MenuDetailsViewModel
                {
                    MenuId = menuResult.Data.MenuId,
                    MenuName = menuResult.Data.MenuName,
                    IsActive = menuResult.Data.IsActive,
                    ImageFileId = menuResult.Data.ImageFileId,
                    ImageFileBytes = menuResult.Data.ImageFileBytes,
                    CafeId = menuResult.Data.CafeId,
                    CafeName = cafeName,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = null,
                    AssignedCategories = assignedCategories,
                    AvailableCategories = availableCategories,
                    CanEdit = true,
                    CanDelete = true,
                    CanManageCategories = true,
                    CanCreateCategory = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Menu Details for ID: {MenuId}", id);
                TempData["Error"] = "Menü detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Menu/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                if (!ValidateUserCafeId(out var cafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new MenuCreateViewModel
                {
                    IsActive = true,
                    CafeId = cafeId
                };

                await LoadCategoriesForViewModel(viewModel, cafeId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Create Menu page");
                TempData["Error"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuCreateViewModel viewModel)
        {
            try
            {
                // Always override with user's CafeId for security
                if (!ValidateUserCafeId(out var cafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                viewModel.CafeId = cafeId;

                // Custom validation
                viewModel.Validate();

                if (!viewModel.IsValid)
                {
                    foreach (var error in viewModel.ValidationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (!ModelState.IsValid)
                {
                    await LoadCategoriesForViewModel(viewModel, cafeId);
                    return View(viewModel);
                }

                // Map ViewModel to DTO
                var menuCreateDto = new MenuCreateDTO
                {
                    MenuName = viewModel.MenuName,
                    IsActive = viewModel.IsActive,
                    CafeId = cafeId,
                    CategoryIds = viewModel.CategoryIds
                };

                byte[]? imageData = null;

                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.ImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var result = await _menuService.CreateAsync(menuCreateDto, imageData);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Message);
                await LoadCategoriesForViewModel(viewModel, cafeId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu");
                ModelState.AddModelError("", "Menü oluşturulurken bir hata oluştu.");

                if (ValidateUserCafeId(out var cafeId))
                {
                    await LoadCategoriesForViewModel(viewModel, cafeId);
                }

                return View(viewModel);
            }
        }

        // GET: Admin/Menu/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz menü ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                var result = await _menuService.GetByIdAsync(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                // Security check: Ensure menu belongs to user's cafe
                if (result.Data.CafeId != userCafeId)
                {
                    _logger.LogWarning("User attempted to edit menu from different cafe. UserId: {UserId}, MenuCafeId: {MenuCafeId}, UserCafeId: {UserCafeId}",
                        User.Identity?.Name, result.Data.CafeId, userCafeId);
                    TempData["Error"] = "Bu menüyü düzenleme yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                // Use Mapster for mapping
                var viewModel = result.Data.Adapt<MenuEditViewModel>();

                await LoadCategoriesForViewModel(viewModel, result.Data.CafeId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Edit page for menu ID: {MenuId}", id);
                TempData["Error"] = "Menü bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Menu/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MenuEditViewModel viewModel)
        {
            try
            {
                // Validate and enforce user's CafeId
                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                // Security check: Ensure the menu being edited belongs to user's cafe
                if (viewModel.CafeId != userCafeId)
                {
                    _logger.LogWarning("User attempted to edit menu from different cafe via POST. UserId: {UserId}",
                        User.Identity?.Name);
                    TempData["Error"] = "Bu menüyü düzenleme yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                // Custom validation
                viewModel.Validate();

                if (!viewModel.IsValid)
                {
                    foreach (var error in viewModel.ValidationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (!ModelState.IsValid)
                {
                    await LoadCategoriesForViewModel(viewModel, viewModel.CafeId);
                    return View(viewModel);
                }

                // Map ViewModel to DTO
                var menuUpdateDto = new MenuUpdateDTO
                {
                    MenuId = viewModel.MenuId,
                    MenuName = viewModel.MenuName,
                    IsActive = viewModel.IsActive,
                    CafeId = userCafeId,
                    ImageFileId = viewModel.ImageFileId,
                    CategoryIds = viewModel.CategoryIds
                };

                byte[]? imageData = null;

                if (viewModel.NewImageFile != null && viewModel.NewImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.NewImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var result = await _menuService.UpdateAsync(menuUpdateDto, imageData);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Message);
                await LoadCategoriesForViewModel(viewModel, viewModel.CafeId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu ID: {MenuId}", viewModel.MenuId);
                ModelState.AddModelError("", "Menü güncellenirken bir hata oluştu.");

                if (ValidateUserCafeId(out var cafeId))
                {
                    await LoadCategoriesForViewModel(viewModel, cafeId);
                }

                return View(viewModel);
            }
        }

        // POST: Admin/Menu/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz menü ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                var menuResult = await _menuService.GetByIdAsync(id);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = menuResult.Message;
                    return RedirectToAction(nameof(Index));
                }

                // Security check: Ensure menu belongs to user's cafe
                if (menuResult.Data.CafeId != userCafeId)
                {
                    _logger.LogWarning("User attempted to delete menu from different cafe. UserId: {UserId}",
                        User.Identity?.Name);
                    TempData["Error"] = "Bu menüyü silme yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                var deleteDto = new MenuDTO
                {
                    MenuId = menuResult.Data.MenuId
                };

                var result = await _menuService.DeleteAsync(deleteDto);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu ID: {MenuId}", id);
                TempData["Error"] = "Menü silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Menu/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Json(new { success = false, message = "Geçersiz menü ID." });
                }

                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return Json(new { success = false, message = "Kullanıcı kafe bilgisi bulunamadı." });
                }

                var menuResult = await _menuService.GetByIdAsync(id);
                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    return Json(new { success = false, message = menuResult.Message });
                }

                // Security check: Ensure menu belongs to user's cafe
                if (menuResult.Data.CafeId != userCafeId)
                {
                    _logger.LogWarning("User attempted to toggle menu from different cafe. UserId: {UserId}",
                        User.Identity?.Name);
                    return Json(new { success = false, message = "Bu menü üzerinde işlem yapma yetkiniz yok." });
                }

                bool newActiveState = !menuResult.Data.IsActive;

                // If activating this menu, first deactivate all other menus for this cafe
                if (newActiveState)
                {
                    var allMenusResult = await _menuService.GetAllAsyncCafesCatsItems(userCafeId);
                    if (allMenusResult.IsSuccess && allMenusResult.Data != null)
                    {
                        foreach (var otherMenu in allMenusResult.Data.Where(m => m.MenuId != id && m.IsActive))
                        {
                            var deactivateDto = new MenuUpdateDTO
                            {
                                MenuId = otherMenu.MenuId,
                                MenuName = otherMenu.MenuName,
                                IsActive = false,
                                CafeId = userCafeId,
                                ImageFileId = otherMenu.ImageFileId,
                                CategoryIds = otherMenu.CategoryIds
                            };

                            await _menuService.UpdateAsync(deactivateDto);
                            _logger.LogInformation("Deactivated menu {MenuId} ({MenuName}) because another menu is being activated",
                                otherMenu.MenuId, otherMenu.MenuName);
                        }
                    }
                }

                // Now update the requested menu
                var updateDto = new MenuUpdateDTO
                {
                    MenuId = menuResult.Data.MenuId,
                    MenuName = menuResult.Data.MenuName,
                    IsActive = newActiveState,
                    CafeId = userCafeId,
                    ImageFileId = menuResult.Data.ImageFileId,
                    CategoryIds = menuResult.Data.CategoryIds
                };

                var result = await _menuService.UpdateAsync(updateDto);

                if (result.IsSuccess)
                {
                    string message = newActiveState
                        ? "Menü aktif hale getirildi. Diğer menüler otomatik olarak pasif yapıldı."
                        : "Menü pasif hale getirildi.";

                    return Json(new
                    {
                        success = true,
                        isActive = newActiveState,
                        message = message
                    });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for menu ID: {MenuId}", id);
                return Json(new { success = false, message = "Durum güncellenirken bir hata oluştu." });
            }
        }

        #endregion

        #region Category Sorting

        // POST: Admin/Menu/UpdateCategoryOrder
        [HttpPost]
        public async Task<IActionResult> UpdateCategoryOrder([FromBody] CategoryOrderUpdateRequest request)
        {
            try
            {
                if (request == null || request.MenuId == Guid.Empty || request.Categories == null || !request.Categories.Any())
                {
                    return Json(new { success = false, message = "Geçersiz istek." });
                }

                _logger.LogInformation("Kategori sıralaması güncelleniyor. MenuId: {MenuId}, Count: {Count}",
                    request.MenuId, request.Categories.Count);

                // Update each category's sort order
                foreach (var categoryOrder in request.Categories)
                {
                    var categoryResult = await _menuCategoryService.GetByIdAsync(categoryOrder.CategoryId);

                    if (categoryResult.IsSuccess && categoryResult.Data != null)
                    {
                        var updateDto = new MenuCategoryUpdateDTO
                        {
                            MenuCategoryId = categoryResult.Data.MenuCategoryId,
                            MenuCategoryName = categoryResult.Data.MenuCategoryName,
                            Description = categoryResult.Data.Description,
                            SortOrder = categoryOrder.SortOrder,
                            CafeId = categoryResult.Data.CafeId,
                            CafeName = categoryResult.Data.CafeName,
                            ImageFileId = categoryResult.Data.ImageFileId,
                            CreatedTime = categoryResult.Data.CreatedTime,
                            UpdatedTime = categoryResult.Data.UpdatedTime
                        };

                        var result = await _menuCategoryService.UpdateAsync(updateDto);

                        if (!result.IsSuccess)
                        {
                            _logger.LogWarning("Kategori sıralaması güncellenemedi. CategoryId: {CategoryId}",
                                categoryOrder.CategoryId);
                        }
                    }
                }

                _logger.LogInformation("Kategori sıralaması başarıyla güncellendi. MenuId: {MenuId}", request.MenuId);
                return Json(new { success = true, message = "Kategori sıralaması başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori sıralaması güncellenirken hata oluştu. MenuId: {MenuId}",
                    request?.MenuId);
                return Json(new { success = false, message = "Sıralama güncellenirken bir hata oluştu." });
            }
        }

        // Helper class for category order update
        public class CategoryOrderUpdateRequest
        {
            public Guid MenuId { get; set; }
            public List<CategoryOrderItem> Categories { get; set; }
        }

        public class CategoryOrderItem
        {
            public Guid CategoryId { get; set; }
            public int SortOrder { get; set; }
        }

        #endregion

        #region Category CRUD (within Menu context)
        // Eski CreateCategory metodunu şununla DEĞİŞTİR:
        // GET: Admin/Menu/AddCategory?menuId={menuId}
        public async Task<IActionResult> AddCategory(Guid menuId)
        {
            try
            {
                if (menuId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz menü ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                var menuResult = await _menuService.GetByIdAsync(menuId);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = menuResult.Message;
                    return RedirectToAction(nameof(Index));
                }

                // Security check
                if (menuResult.Data.CafeId != userCafeId)
                {
                    TempData["Error"] = "Bu menüye kategori ekleme yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                // Kafenin TÜM kategorilerini getir
                var allCategoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(userCafeId);

                // Bu menüde OLMAYAN kategorileri filtrele
                var existingCategoryIds = menuResult.Data.CategoryIds ?? new List<Guid>();
                var availableCategories = allCategoriesResult.Data?
                    .Where(c => !existingCategoryIds.Contains(c.MenuCategoryId))
                    .ToList() ?? new List<MenuCategoryListDTO>();

                var viewModel = new CategoryAddToMenuViewModel
                {
                    MenuId = menuId,
                    MenuName = menuResult.Data.MenuName,
                    CafeId = userCafeId,
                    AvailableCategories = new SelectList(availableCategories, "MenuCategoryId", "MenuCategoryName"),
                    NewCategorySortOrder = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading AddCategory page for MenuId: {MenuId}", menuId);
                TempData["Error"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = menuId });
            }
        }

        // POST: Admin/Menu/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryAddToMenuViewModel viewModel)
        {
            try
            {
                // Validate and enforce user's CafeId
                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                // Security check
                if (viewModel.CafeId != userCafeId)
                {
                    TempData["Error"] = "Güvenlik hatası.";
                    return RedirectToAction(nameof(Details), new { id = viewModel.MenuId });
                }

                // Custom validation
                viewModel.Validate();

                if (!viewModel.IsValid)
                {
                    foreach (var error in viewModel.ValidationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (!ModelState.IsValid)
                {
                    // Reload available categories
                    var allCategoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(userCafeId);
                    var menuResult = await _menuService.GetByIdAsync(viewModel.MenuId);
                    var existingCategoryIds = menuResult.Data?.CategoryIds ?? new List<Guid>();
                    var availableCategories = allCategoriesResult.Data?
                        .Where(c => !existingCategoryIds.Contains(c.MenuCategoryId))
                        .ToList() ?? new List<MenuCategoryListDTO>();

                    viewModel.AvailableCategories = new SelectList(availableCategories, "MenuCategoryId", "MenuCategoryName");
                    return View(viewModel);
                }

                // SENARYO 1: Mevcut kategori seçildi
                if (viewModel.IsSelectingExisting)
                {
                    var assignResult = await _menuService.AssignCategoryToMenuAsync(
                        viewModel.MenuId,
                        viewModel.SelectedExistingCategoryId!.Value);

                    if (assignResult.IsSuccess)
                    {
                        TempData["Success"] = "Kategori menüye başarıyla eklendi.";
                        return RedirectToAction(nameof(Details), new { id = viewModel.MenuId });
                    }

                    ModelState.AddModelError("", assignResult.Message);

                    // Reload categories
                    var allCategoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(userCafeId);
                    var menuResult = await _menuService.GetByIdAsync(viewModel.MenuId);
                    var existingCategoryIds = menuResult.Data?.CategoryIds ?? new List<Guid>();
                    var availableCategories = allCategoriesResult.Data?
                        .Where(c => !existingCategoryIds.Contains(c.MenuCategoryId))
                        .ToList() ?? new List<MenuCategoryListDTO>();

                    viewModel.AvailableCategories = new SelectList(availableCategories, "MenuCategoryId", "MenuCategoryName");
                    return View(viewModel);
                }

                // SENARYO 2: Yeni kategori yaratılacak
                if (viewModel.IsCreatingNew)
                {
                    var categoryCreateDto = new MenuCategoryCreateDTO
                    {
                        MenuCategoryName = viewModel.NewCategoryName!,
                        Description = viewModel.NewCategoryDescription,
                        SortOrder = viewModel.NewCategorySortOrder,
                        CafeId = userCafeId,
                        CafeName = ""
                    };

                    byte[]? imageData = null;
                    if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await viewModel.ImageFile.CopyToAsync(memoryStream);
                            imageData = memoryStream.ToArray();
                        }
                    }

                    var createResult = await _menuCategoryService.CreateAsync(categoryCreateDto, imageData);

                    if (createResult.IsSuccess && createResult.Data != null)
                    {
                        // Yeni yaratılan kategoriyi menüye ata
                        var assignResult = await _menuService.AssignCategoryToMenuAsync(
                            viewModel.MenuId,
                            createResult.Data.MenuCategoryId);

                        if (assignResult.IsSuccess)
                        {
                            TempData["Success"] = "Yeni kategori yaratıldı ve menüye eklendi.";
                            return RedirectToAction(nameof(Details), new { id = viewModel.MenuId });
                        }

                        TempData["Warning"] = "Kategori yaratıldı ama menüye eklenemedi: " + assignResult.Message;
                        return RedirectToAction(nameof(Details), new { id = viewModel.MenuId });
                    }

                    ModelState.AddModelError("", createResult.Message);

                    // Reload categories
                    var allCategoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(userCafeId);
                    var menuResult = await _menuService.GetByIdAsync(viewModel.MenuId);
                    var existingCategoryIds = menuResult.Data?.CategoryIds ?? new List<Guid>();
                    var availableCategories = allCategoriesResult.Data?
                        .Where(c => !existingCategoryIds.Contains(c.MenuCategoryId))
                        .ToList() ?? new List<MenuCategoryListDTO>();

                    viewModel.AvailableCategories = new SelectList(availableCategories, "MenuCategoryId", "MenuCategoryName");
                    return View(viewModel);
                }

                ModelState.AddModelError("", "Lütfen mevcut bir kategori seçin veya yeni kategori adı girin.");

                // Reload categories
                var allCats = await _menuCategoryService.GetAllAsyncCafesCats(userCafeId);
                var menu = await _menuService.GetByIdAsync(viewModel.MenuId);
                var existingIds = menu.Data?.CategoryIds ?? new List<Guid>();
                var available = allCats.Data?
                    .Where(c => !existingIds.Contains(c.MenuCategoryId))
                    .ToList() ?? new List<MenuCategoryListDTO>();

                viewModel.AvailableCategories = new SelectList(available, "MenuCategoryId", "MenuCategoryName");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding category for MenuId: {MenuId}", viewModel.MenuId);
                ModelState.AddModelError("", "Kategori eklenirken bir hata oluştu.");
                return View(viewModel);
            }
        }

        // ESKİ DeleteCategory metodunu şununla DEĞİŞTİR:
        // POST: Admin/Menu/RemoveCategoryFromMenu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCategoryFromMenu(Guid menuId, Guid categoryId)
        {
            try
            {
                if (menuId == Guid.Empty || categoryId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz ID.";
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                var menuResult = await _menuService.GetByIdAsync(menuId);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = "Menü bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Security check
                if (menuResult.Data.CafeId != userCafeId)
                {
                    TempData["Error"] = "Bu menü üzerinde işlem yapma yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                // Sadece menü-kategori ilişkisini kaldır, kategoriyi silme!
                var result = await _menuService.RemoveCategoryFromMenuAsync(menuId, categoryId);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Details), new { id = menuId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing category from menu. MenuId: {MenuId}, CategoryId: {CategoryId}",
                    menuId, categoryId);
                TempData["Error"] = "Kategori menüden çıkarılırken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = menuId });
            }
        }
        // GET: Admin/Menu/CategoryDetails?menuId={menuId}&categoryId={categoryId}
        public async Task<IActionResult> CategoryDetails(Guid menuId, Guid categoryId)
        {
            try
            {
                if (menuId == Guid.Empty || categoryId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz ID.";
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                if (!ValidateUserCafeId(out var userCafeId, out var cafeName))
                {
                    return RedirectToAction(nameof(Index));
                }

                var menuResult = await _menuService.GetByIdAsync(menuId);
                var categoryResult = await _menuCategoryService.GetByIdAsync(categoryId);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = "Menü bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Security check: Ensure menu belongs to user's cafe
                if (menuResult.Data.CafeId != userCafeId)
                {
                    TempData["Error"] = "Bu menüye erişim yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                if (!categoryResult.IsSuccess || categoryResult.Data == null)
                {
                    TempData["Error"] = categoryResult.Message;
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                // Security check: Ensure category belongs to user's cafe
                if (categoryResult.Data.CafeId != userCafeId)
                {
                    TempData["Error"] = "Bu kategoriye erişim yetkiniz yok.";
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                // Get items for this category
                var itemsResult = await _menuItemService.GetAllAsyncCafesCatsItems(categoryResult.Data.CafeId);

                // Filter items by this specific category
                var categoryItems = itemsResult.Data?
                    .Where(i => i.MenuCategoryId == categoryId)
                    .OrderBy(i => i.SortOrder)
                    .ToList() ?? new List<MenuItemListDTO>();

                // Use Mapster for mapping items
                var itemViewModels = categoryItems.Adapt<List<ItemListItemViewModel>>();

                // Map to ViewModel
                var viewModel = new CategoryDetailsViewModel
                {
                    MenuId = menuId,
                    MenuName = menuResult.Data.MenuName,
                    CategoryId = categoryResult.Data.MenuCategoryId,
                    CategoryName = categoryResult.Data.MenuCategoryName,
                    Description = categoryResult.Data.Description,
                    SortOrder = categoryResult.Data.SortOrder,
                    CafeId = categoryResult.Data.CafeId,
                    CafeName = cafeName,
                    ImageFileId = categoryResult.Data.ImageFileId,
                    ImageFileBytes = categoryResult.Data.ImageFileBytes,
                    CreatedTime = categoryResult.Data.CreatedTime,
                    UpdatedTime = categoryResult.Data.UpdatedTime,
                    Items = itemViewModels,
                    CanEdit = true,
                    CanDelete = true,
                    CanManageItems = true,
                    CanCreateItem = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryDetails for MenuId: {MenuId}, CategoryId: {CategoryId}", menuId, categoryId);
                TempData["Error"] = "Kategori detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = menuId });
            }
        }

        // GET: Admin/Menu/CreateCategory?menuId={menuId}
        public async Task<IActionResult> CreateCategory(Guid menuId)
        {
            try
            {
                if (menuId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz menü ID.";
                    return RedirectToAction(nameof(Index));
                }

                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                var menuResult = await _menuService.GetByIdAsync(menuId);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = menuResult.Message;
                    return RedirectToAction(nameof(Index));
                }

                // Security check
                if (menuResult.Data.CafeId != userCafeId)
                {
                    TempData["Error"] = "Bu menüye kategori ekleme yetkiniz yok.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CategoryCreateViewModel
                {
                    MenuId = menuId,
                    MenuName = menuResult.Data.MenuName,
                    CafeId = userCafeId,
                    SortOrder = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CreateCategory page for MenuId: {MenuId}", menuId);
                TempData["Error"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = menuId });
            }
        }

        // POST: Admin/Menu/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryCreateViewModel viewModel)
        {
            try
            {
                // Validate and enforce user's CafeId
                if (!ValidateUserCafeId(out var userCafeId))
                {
                    return RedirectToAction(nameof(Index));
                }

                // Security check
                if (viewModel.CafeId != userCafeId)
                {
                    TempData["Error"] = "Güvenlik hatası: CafeId uyuşmazlığı.";
                    return RedirectToAction(nameof(Details), new { id = viewModel.MenuId });
                }

                // Custom validation
                viewModel.Validate();

                if (!viewModel.IsValid)
                {
                    foreach (var error in viewModel.ValidationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                // Map ViewModel to DTO
                var categoryCreateDto = new MenuCategoryCreateDTO
                {
                    MenuCategoryName = viewModel.CategoryName,
                    Description = viewModel.Description,
                    SortOrder = viewModel.SortOrder,
                    CafeId = userCafeId,
                    CafeName = ""
                };

                byte[]? imageData = null;

                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.ImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var result = await _menuCategoryService.CreateAsync(categoryCreateDto, imageData);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Details), new { id = viewModel.MenuId });
                }

                ModelState.AddModelError("", result.Message);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category for MenuId: {MenuId}", viewModel.MenuId);
                ModelState.AddModelError("", "Kategori oluşturulurken bir hata oluştu.");
                return View(viewModel);
            }
        }

        // GET: Admin/Menu/EditCategory?menuId={menuId}&categoryId={categoryId}
        public async Task<IActionResult> EditCategory(Guid menuId, Guid categoryId)
        {
            try
            {
                if (menuId == Guid.Empty || categoryId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz ID.";
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                var menuResult = await _menuService.GetByIdAsync(menuId);
                var categoryResult = await _menuCategoryService.GetByIdAsync(categoryId);

                if (!categoryResult.IsSuccess || categoryResult.Data == null)
                {
                    TempData["Error"] = categoryResult.Message;
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                // Use Mapster for mapping
                var viewModel = categoryResult.Data.Adapt<CategoryEditViewModel>();

                // Set menu-specific properties
                viewModel.MenuId = menuId;
                viewModel.MenuName = menuResult.Data?.MenuName ?? "Menü";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading EditCategory page for CategoryId: {CategoryId}", categoryId);
                TempData["Error"] = "Kategori bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = menuId });
            }
        }

        // POST: Admin/Menu/EditCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryEditViewModel viewModel)
        {
            try
            {
                // Custom validation
                viewModel.Validate();

                if (!viewModel.IsValid)
                {
                    foreach (var error in viewModel.ValidationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                // Map ViewModel to DTO
                var categoryUpdateDto = new MenuCategoryUpdateDTO
                {
                    MenuCategoryId = viewModel.CategoryId,
                    MenuCategoryName = viewModel.CategoryName,
                    Description = viewModel.Description,
                    SortOrder = viewModel.SortOrder,
                    CafeId = viewModel.CafeId,
                    CafeName = "",
                    ImageFileId = viewModel.ImageFileId,
                    CreatedTime = viewModel.CreatedTime,
                    UpdatedTime = viewModel.UpdatedTime
                };

                byte[]? imageData = null;

                if (viewModel.NewImageFile != null && viewModel.NewImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.NewImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var result = await _menuCategoryService.UpdateAsync(categoryUpdateDto, imageData);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Details), new { id = viewModel.MenuId });
                }

                ModelState.AddModelError("", result.Message);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category ID: {CategoryId}", viewModel.CategoryId);
                ModelState.AddModelError("", "Kategori güncellenirken bir hata oluştu.");
                return View(viewModel);
            }
        }

        // POST: Admin/Menu/DeleteCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(Guid menuId, Guid categoryId)
        {
            try
            {
                if (menuId == Guid.Empty || categoryId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz ID.";
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                var categoryResult = await _menuCategoryService.GetByIdAsync(categoryId);

                if (!categoryResult.IsSuccess || categoryResult.Data == null)
                {
                    TempData["Error"] = categoryResult.Message;
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                var deleteDto = new MenuCategoryDTO
                {
                    MenuCategoryId = categoryResult.Data.MenuCategoryId
                };

                var result = await _menuCategoryService.DeleteAsync(deleteDto);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Details), new { id = menuId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category ID: {CategoryId}", categoryId);
                TempData["Error"] = "Kategori silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = menuId });
            }
        }

        #endregion

        #region MenuItem CRUD (within Category context)

        // GET: Admin/Menu/CreateItem?menuId={menuId}&categoryId={categoryId}
        public async Task<IActionResult> CreateItem(Guid menuId, Guid categoryId)
        {
            try
            {
                if (menuId == Guid.Empty || categoryId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz ID.";
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                var menuResult = await _menuService.GetByIdAsync(menuId);
                var categoryResult = await _menuCategoryService.GetByIdAsync(categoryId);

                if (!categoryResult.IsSuccess || categoryResult.Data == null)
                {
                    TempData["Error"] = categoryResult.Message;
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                var viewModel = new ItemCreateViewModel
                {
                    MenuId = menuId,
                    MenuName = menuResult.Data?.MenuName ?? "Menü",
                    CategoryId = categoryId,
                    CategoryName = categoryResult.Data.MenuCategoryName,
                    SortOrder = 0,
                    Price = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CreateItem page");
                TempData["Error"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(CategoryDetails), new { menuId, categoryId });
            }
        }

        // POST: Admin/Menu/CreateItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(ItemCreateViewModel viewModel)
        {
            try
            {
                // Custom validation
                viewModel.Validate();

                if (!viewModel.IsValid && !viewModel.HasWarnings)
                {
                    foreach (var error in viewModel.ValidationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else if (viewModel.HasWarnings)
                {
                    // Add warnings as informational messages
                    foreach (var warning in viewModel.ValidationErrors.Where(e => e.Contains("önerilir")))
                    {
                        TempData["Warning"] = warning;
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                // Map ViewModel to DTO
                var itemCreateDto = new MenuItemCreateDTO
                {
                    MenuItemName = viewModel.ItemName,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    SortOrder = viewModel.SortOrder,
                    MenuCategoryId = viewModel.CategoryId,
                    MenuCategoryName = viewModel.CategoryName
                };

                byte[]? imageData = null;

                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.ImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var result = await _menuItemService.CreateAsync(itemCreateDto, imageData);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(CategoryDetails), new { menuId = viewModel.MenuId, categoryId = viewModel.CategoryId });
                }

                ModelState.AddModelError("", result.Message);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu item");
                ModelState.AddModelError("", "Ürün oluşturulurken bir hata oluştu.");
                return View(viewModel);
            }
        }

        // GET: Admin/Menu/EditItem?menuId={menuId}&categoryId={categoryId}&itemId={itemId}
        public async Task<IActionResult> EditItem(Guid menuId, Guid categoryId, Guid itemId)
        {
            try
            {
                if (menuId == Guid.Empty || categoryId == Guid.Empty || itemId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz ID.";
                    return RedirectToAction(nameof(CategoryDetails), new { menuId, categoryId });
                }

                var menuResult = await _menuService.GetByIdAsync(menuId);
                var categoryResult = await _menuCategoryService.GetByIdAsync(categoryId);
                var itemResult = await _menuItemService.GetByIdAsync(itemId);

                if (!itemResult.IsSuccess || itemResult.Data == null)
                {
                    TempData["Error"] = itemResult.Message;
                    return RedirectToAction(nameof(CategoryDetails), new { menuId, categoryId });
                }

                // Use Mapster for mapping
                var viewModel = itemResult.Data.Adapt<ItemEditViewModel>();

                // Set menu and category specific properties
                viewModel.MenuId = menuId;
                viewModel.MenuName = menuResult.Data?.MenuName ?? "Menü";
                viewModel.CategoryId = categoryId;
                viewModel.CategoryName = categoryResult.Data?.MenuCategoryName ?? "Kategori";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading EditItem page for ItemId: {ItemId}", itemId);
                TempData["Error"] = "Ürün bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(CategoryDetails), new { menuId, categoryId });
            }
        }

        // POST: Admin/Menu/EditItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(ItemEditViewModel viewModel)
        {
            try
            {
                // Custom validation
                viewModel.Validate();

                if (!viewModel.IsValid && !viewModel.HasWarnings)
                {
                    foreach (var error in viewModel.ValidationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (viewModel.HasWarnings)
                {
                    // Add warnings as informational messages
                    foreach (var warning in viewModel.ValidationWarnings)
                    {
                        TempData["Warning"] = TempData["Warning"] != null
                            ? TempData["Warning"] + " | " + warning
                            : warning;
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                // Map ViewModel to DTO
                var itemUpdateDto = new MenuItemUpdateDTO
                {
                    MenuItemId = viewModel.ItemId,
                    MenuItemName = viewModel.ItemName,
                    Description = viewModel.Description,
                    Price = viewModel.Price,
                    SortOrder = viewModel.SortOrder,
                    MenuCategoryId = viewModel.CategoryId,
                    MenuCategoryName = viewModel.CategoryName,
                    ImageFileId = viewModel.ImageFileId
                };

                byte[]? imageData = null;

                if (viewModel.NewImageFile != null && viewModel.NewImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.NewImageFile.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }
                }

                var result = await _menuItemService.UpdateAsync(itemUpdateDto, imageData);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(CategoryDetails), new { menuId = viewModel.MenuId, categoryId = viewModel.CategoryId });
                }

                ModelState.AddModelError("", result.Message);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item ID: {ItemId}", viewModel.ItemId);
                ModelState.AddModelError("", "Ürün güncellenirken bir hata oluştu.");
                return View(viewModel);
            }
        }

        // POST: Admin/Menu/DeleteItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(Guid menuId, Guid categoryId, Guid itemId)
        {
            try
            {
                if (menuId == Guid.Empty || categoryId == Guid.Empty || itemId == Guid.Empty)
                {
                    TempData["Error"] = "Geçersiz ID.";
                    return RedirectToAction(nameof(CategoryDetails), new { menuId, categoryId });
                }

                var itemResult = await _menuItemService.GetByIdAsync(itemId);

                if (!itemResult.IsSuccess || itemResult.Data == null)
                {
                    TempData["Error"] = itemResult.Message;
                    return RedirectToAction(nameof(CategoryDetails), new { menuId, categoryId });
                }

                var deleteDto = new MenuItemDTO
                {
                    MenuItemId = itemResult.Data.MenuItemId
                };

                var result = await _menuItemService.DeleteAsync(deleteDto);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(CategoryDetails), new { menuId, categoryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu item ID: {ItemId}", itemId);
                TempData["Error"] = "Ürün silinirken bir hata oluştu.";
                return RedirectToAction(nameof(CategoryDetails), new { menuId, categoryId });
            }
        }

        #endregion

        #region Item Sorting

        // POST: Admin/Menu/UpdateItemOrder
        [HttpPost]
        public async Task<IActionResult> UpdateItemOrder([FromBody] ItemOrderUpdateRequest request)
        {
            try
            {
                if (request == null || request.CategoryId == Guid.Empty || request.Items == null || !request.Items.Any())
                {
                    return Json(new { success = false, message = "Geçersiz istek." });
                }

                _logger.LogInformation("Ürün sıralaması güncelleniyor. CategoryId: {CategoryId}, Count: {Count}",
                    request.CategoryId, request.Items.Count);

                // Update each item's sort order
                foreach (var itemOrder in request.Items)
                {
                    var itemResult = await _menuItemService.GetByIdAsync(itemOrder.ItemId);

                    if (itemResult.IsSuccess && itemResult.Data != null)
                    {
                        var updateDto = new MenuItemUpdateDTO
                        {
                            MenuItemId = itemResult.Data.MenuItemId,
                            MenuItemName = itemResult.Data.MenuItemName,
                            Description = itemResult.Data.Description,
                            Price = itemResult.Data.Price,
                            SortOrder = itemOrder.SortOrder,
                            MenuCategoryId = itemResult.Data.MenuCategoryId,
                            MenuCategoryName = itemResult.Data.MenuCategoryName,
                            ImageFileId = itemResult.Data.ImageFileId
                        };

                        var result = await _menuItemService.UpdateAsync(updateDto);

                        if (!result.IsSuccess)
                        {
                            _logger.LogWarning("Ürün sıralaması güncellenemedi. ItemId: {ItemId}",
                                itemOrder.ItemId);
                        }
                    }
                }

                _logger.LogInformation("Ürün sıralaması başarıyla güncellendi. CategoryId: {CategoryId}", request.CategoryId);
                return Json(new { success = true, message = "Ürün sıralaması başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sıralaması güncellenirken hata oluştu. CategoryId: {CategoryId}",
                    request?.CategoryId);
                return Json(new { success = false, message = "Sıralama güncellenirken bir hata oluştu." });
            }
        }

        // Helper class for item order update
        public class ItemOrderUpdateRequest
        {
            public Guid CategoryId { get; set; }
            public List<ItemOrderItem> Items { get; set; }
        }

        public class ItemOrderItem
        {
            public Guid ItemId { get; set; }
            public int SortOrder { get; set; }
        }

        #endregion

        #region Helper Methods

        private async Task LoadCategoriesForViewModel(MenuCreateViewModel viewModel, Guid cafeId)
        {
            var categoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(cafeId);
            viewModel.Categories = categoriesResult.IsSuccess && categoriesResult.Data != null
                ? new MultiSelectList(categoriesResult.Data, "MenuCategoryId", "MenuCategoryName")
                : new MultiSelectList(Enumerable.Empty<MenuCategoryListDTO>());
        }

        private async Task LoadCategoriesForViewModel(MenuEditViewModel viewModel, Guid cafeId)
        {
            var categoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(cafeId);
            viewModel.Categories = categoriesResult.IsSuccess && categoriesResult.Data != null
                ? new MultiSelectList(categoriesResult.Data, "MenuCategoryId", "MenuCategoryName", viewModel.CategoryIds)
                : new MultiSelectList(Enumerable.Empty<MenuCategoryListDTO>());
        }

        #endregion
    }
}