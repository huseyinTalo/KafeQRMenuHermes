using KafeQRMenu.BLogic.DTOs.CafeDTOs;
using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.DTOs.MenuDTOs;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.BLogic.Services.MenuService;
using KafeQRMenu.UI.Areas.Admin.ViewModels.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KafeQRMenu.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize] // Add specific roles if needed: [Authorize(Roles = "Admin,SuperAdmin")]
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly IMenuItemService _menuItemService;
        private readonly ICafeService _cafeService;
        private readonly ILogger<MenuController> _logger;

        public MenuController(
            IMenuService menuService,
            IMenuCategoryService menuCategoryService,
            IMenuItemService menuItemService,
            ICafeService cafeService,
            ILogger<MenuController> logger)
        {
            _menuService = menuService;
            _menuCategoryService = menuCategoryService;
            _menuItemService = menuItemService;
            _cafeService = cafeService;
            _logger = logger;
        }

        #region Menu CRUD

        // GET: Admin/Menu/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var result = await _menuService.GetAllAsync();

                if (!result.IsSuccess)
                {
                    TempData["Error"] = result.Message;
                    return View(new MenuIndexViewModel());
                }

                // Map to ViewModel
                var viewModel = new MenuIndexViewModel
                {
                    Menus = result.Data?.Select(m => new MenuListItemViewModel
                    {
                        MenuId = m.MenuId,
                        MenuName = m.MenuName,
                        IsActive = m.IsActive,
                        ImageFileId = m.ImageFileId,
                        ImageFileBytes = m.ImageFileBytes,
                        CafeId = m.CafeId,
                        CafeName = "Kafe", // TODO: Get from cafe service if needed
                        CategoryCount = 0, // TODO: Add if needed
                        CreatedTime = DateTime.Now, // TODO: Add to DTO
                        CanEdit = true,
                        CanDelete = true,
                        CanViewDetails = true,
                        CanToggleActive = true
                    }).ToList() ?? new List<MenuListItemViewModel>(),
                    CanCreate = true,
                    TotalCount = result.Data?.Count ?? 0
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

                var menuResult = await _menuService.GetByIdAsync(id);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = menuResult.Message;
                    return RedirectToAction(nameof(Index));
                }

                // Get categories for this menu's cafe
                var categoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(menuResult.Data.CafeId);

                // Map to ViewModel
                var viewModel = new MenuDetailsViewModel
                {
                    MenuId = menuResult.Data.MenuId,
                    MenuName = menuResult.Data.MenuName,
                    IsActive = menuResult.Data.IsActive,
                    ImageFileId = menuResult.Data.ImageFileId,
                    ImageFileBytes = menuResult.Data.ImageFileBytes,
                    CafeId = menuResult.Data.CafeId,
                    CafeName = "Kafe", // TODO: Get from menu result if available
                    CreatedTime = DateTime.Now, // TODO: Add to DTO
                    UpdatedTime = null,
                    AssignedCategories = menuResult.Data.Categories?.Select(c => new CategoryListItemViewModel
                    {
                        CategoryId = c.MenuCategoryId,
                        CategoryName = c.MenuCategoryName,
                        Description = c.Description,
                        SortOrder = c.SortOrder,
                        ImageFileId = c.ImageFileId,
                        ImageFileBytes = c.ImageFileBytes,
                        ItemCount = 0, // TODO: Get from service if needed
                        IsAssignedToMenu = true,
                        CanEdit = true,
                        CanDelete = true,
                        CanViewDetails = true
                    }).ToList() ?? new List<CategoryListItemViewModel>(),
                    AvailableCategories = categoriesResult.Data?.Where(c =>
                        !menuResult.Data.CategoryIds?.Contains(c.MenuCategoryId) ?? true
                    ).Select(c => new CategoryListItemViewModel
                    {
                        CategoryId = c.MenuCategoryId,
                        CategoryName = c.MenuCategoryName,
                        Description = c.Description,
                        SortOrder = c.SortOrder,
                        ImageFileId = c.ImageFileId,
                        ImageFileBytes = c.ImageFileBytes,
                        ItemCount = 0,
                        IsAssignedToMenu = false,
                        CanEdit = true,
                        CanDelete = true,
                        CanViewDetails = true
                    }).ToList() ?? new List<CategoryListItemViewModel>(),
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
                var viewModel = new MenuCreateViewModel
                {
                    IsActive = true
                };

                await LoadCafesForViewModel(viewModel);
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
                    await LoadCafesForViewModel(viewModel);
                    return View(viewModel);
                }

                // Map ViewModel to DTO
                var menuCreateDto = new MenuCreateDTO
                {
                    MenuName = viewModel.MenuName,
                    IsActive = viewModel.IsActive,
                    CafeId = viewModel.CafeId,
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
                await LoadCafesForViewModel(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu");
                ModelState.AddModelError("", "Menü oluşturulurken bir hata oluştu.");
                await LoadCafesForViewModel(viewModel);
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

                var result = await _menuService.GetByIdAsync(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                // Map to ViewModel
                var viewModel = new MenuEditViewModel
                {
                    MenuId = result.Data.MenuId,
                    MenuName = result.Data.MenuName,
                    IsActive = result.Data.IsActive,
                    CafeId = result.Data.CafeId,
                    ImageFileId = result.Data.ImageFileId,
                    CategoryIds = result.Data.CategoryIds,
                    CurrentImageBytes = result.Data.ImageFileBytes
                };

                await LoadCafesForViewModel(viewModel);
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
                    await LoadCafesForViewModel(viewModel);
                    await LoadCategoriesForViewModel(viewModel, viewModel.CafeId);
                    return View(viewModel);
                }

                // Map ViewModel to DTO
                var menuUpdateDto = new MenuUpdateDTO
                {
                    MenuId = viewModel.MenuId,
                    MenuName = viewModel.MenuName,
                    IsActive = viewModel.IsActive,
                    CafeId = viewModel.CafeId,
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
                await LoadCafesForViewModel(viewModel);
                await LoadCategoriesForViewModel(viewModel, viewModel.CafeId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu ID: {MenuId}", viewModel.MenuId);
                ModelState.AddModelError("", "Menü güncellenirken bir hata oluştu.");
                await LoadCafesForViewModel(viewModel);
                await LoadCategoriesForViewModel(viewModel, viewModel.CafeId);
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

                var menuResult = await _menuService.GetByIdAsync(id);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = menuResult.Message;
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

                var menuResult = await _menuService.GetByIdAsync(id);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    return Json(new { success = false, message = menuResult.Message });
                }

                var updateDto = new MenuUpdateDTO
                {
                    MenuId = menuResult.Data.MenuId,
                    MenuName = menuResult.Data.MenuName,
                    IsActive = !menuResult.Data.IsActive, // Toggle
                    CafeId = menuResult.Data.CafeId,
                    ImageFileId = menuResult.Data.ImageFileId,
                    CategoryIds = menuResult.Data.CategoryIds
                };

                var result = await _menuService.UpdateAsync(updateDto);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, isActive = updateDto.IsActive, message = result.Message });
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

        #region Category CRUD (within Menu context)

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

                var menuResult = await _menuService.GetByIdAsync(menuId);
                var categoryResult = await _menuCategoryService.GetByIdAsync(categoryId);

                if (!categoryResult.IsSuccess || categoryResult.Data == null)
                {
                    TempData["Error"] = categoryResult.Message;
                    return RedirectToAction(nameof(Details), new { id = menuId });
                }

                // Get items for this category
                var itemsResult = await _menuItemService.GetAllAsyncCafesCatsItems(categoryResult.Data.CafeId);

                // Filter items by this specific category
                var categoryItems = itemsResult.Data?
                    .Where(i => i.MenuCategoryId == categoryId)
                    .OrderBy(i => i.SortOrder)
                    .ToList() ?? new List<MenuItemListDTO>();

                // Map to ViewModel
                var viewModel = new CategoryDetailsViewModel
                {
                    MenuId = menuId,
                    MenuName = menuResult.Data?.MenuName ?? "Menü",
                    CategoryId = categoryResult.Data.MenuCategoryId,
                    CategoryName = categoryResult.Data.MenuCategoryName,
                    Description = categoryResult.Data.Description,
                    SortOrder = categoryResult.Data.SortOrder,
                    CafeId = categoryResult.Data.CafeId,
                    CafeName = categoryResult.Data.CafeName,
                    ImageFileId = categoryResult.Data.ImageFileId,
                    ImageFileBytes = categoryResult.Data.ImageFileBytes,
                    CreatedTime = categoryResult.Data.CreatedTime,
                    UpdatedTime = categoryResult.Data.UpdatedTime,
                    Items = categoryItems.Select(i => new ItemListItemViewModel
                    {
                        ItemId = i.MenuItemId,
                        ItemName = i.MenuItemName,
                        Description = i.Description,
                        Price = i.Price,
                        SortOrder = i.SortOrder,
                        ImageFileId = i.ImageFileId,
                        ImageFileBytes = i.ImageFileBytes,
                        CreatedTime = i.CreatedTime,
                        CanEdit = true,
                        CanDelete = true
                    }).ToList(),
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

                var menuResult = await _menuService.GetByIdAsync(menuId);

                if (!menuResult.IsSuccess || menuResult.Data == null)
                {
                    TempData["Error"] = menuResult.Message;
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CategoryCreateViewModel
                {
                    MenuId = menuId,
                    MenuName = menuResult.Data.MenuName,
                    CafeId = menuResult.Data.CafeId,
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
                    CafeId = viewModel.CafeId,
                    CafeName = "" // Will be populated by service
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

                var viewModel = new CategoryEditViewModel
                {
                    MenuId = menuId,
                    MenuName = menuResult.Data?.MenuName ?? "Menü",
                    CategoryId = categoryResult.Data.MenuCategoryId,
                    CategoryName = categoryResult.Data.MenuCategoryName,
                    Description = categoryResult.Data.Description,
                    SortOrder = categoryResult.Data.SortOrder,
                    CafeId = categoryResult.Data.CafeId,
                    ImageFileId = categoryResult.Data.ImageFileId,
                    CurrentImageBytes = categoryResult.Data.ImageFileBytes,
                    CreatedTime = categoryResult.Data.CreatedTime,
                    UpdatedTime = categoryResult.Data.UpdatedTime
                };

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
                    CafeName = "", // Will be populated by service
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

                var viewModel = new ItemEditViewModel
                {
                    MenuId = menuId,
                    MenuName = menuResult.Data?.MenuName ?? "Menü",
                    CategoryId = categoryId,
                    CategoryName = categoryResult.Data?.MenuCategoryName ?? "Kategori",
                    ItemId = itemResult.Data.MenuItemId,
                    ItemName = itemResult.Data.MenuItemName,
                    Description = itemResult.Data.Description,
                    Price = itemResult.Data.Price,
                    OriginalPrice = itemResult.Data.Price, // For tracking changes
                    SortOrder = itemResult.Data.SortOrder,
                    ImageFileId = itemResult.Data.ImageFileId,
                    CurrentImageBytes = itemResult.Data.ImageFileBytes,
                    CreatedTime = itemResult.Data.CreatedTime,
                    UpdatedTime = itemResult.Data.UpdatedTime
                };

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

        #region Helper Methods

        private async Task LoadCafesForViewModel(MenuCreateViewModel viewModel)
        {
            var cafesResult = await _cafeService.GetAllAsync();
            viewModel.Cafes = cafesResult.IsSuccess && cafesResult.Data != null
                ? new SelectList(cafesResult.Data, "Id", "CafeName")
                : new SelectList(Enumerable.Empty<CafeListDTO>());
        }

        private async Task LoadCafesForViewModel(MenuEditViewModel viewModel)
        {
            var cafesResult = await _cafeService.GetAllAsync();
            viewModel.Cafes = cafesResult.IsSuccess && cafesResult.Data != null
                ? new SelectList(cafesResult.Data, "Id", "CafeName", viewModel.CafeId)
                : new SelectList(Enumerable.Empty<CafeListDTO>());
        }

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