using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.UI.Areas.Admin.Models.MenuPreviewVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KafeQRMenu.BLogic.Services.MenuService;

namespace KafeQRMenu.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuPreviewController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly IMenuItemService _menuItemService;
        private readonly ILogger<MenuPreviewController> _logger;

        public MenuPreviewController(
            IMenuService menuService,
            IMenuCategoryService menuCategoryService,
            IMenuItemService menuItemService,
            ILogger<MenuPreviewController> logger)
        {
            _menuService = menuService;
            _menuCategoryService = menuCategoryService;
            _menuItemService = menuItemService;
            _logger = logger;
        }

        // GET: Admin/MenuPreview?menuId=xxx
        [HttpGet]
        public async Task<IActionResult> Index(Guid? menuId)
        {
            try
            {
                var cafeIdClaim = User.FindFirst("CafeId")?.Value;
                if (!Guid.TryParse(cafeIdClaim, out Guid cafeId))
                {
                    TempData["ErrorMessage"] = "Kafe bilgisi alınamadı.";
                    return RedirectToAction("Index", "Home");
                }

                var cafeName = User.FindFirst("CafeName")?.Value ?? "Menü";

                // Get all menus for this cafe
                var allMenusResult = await _menuService.GetAllAsyncCafesCatsItems(cafeId);

                if (!allMenusResult.IsSuccess || allMenusResult.Data == null || !allMenusResult.Data.Any())
                {
                    TempData["WarningMessage"] = "Henüz hiç menü oluşturulmamış. Lütfen önce bir menü oluşturun.";
                    return View(new MenuPreviewViewModel
                    {
                        CafeName = cafeName,
                        MenuName = "Menü Yok"
                    });
                }

                // Convert to selection items
                var availableMenus = allMenusResult.Data
                    .OrderByDescending(m => m.IsActive)
                    .ThenByDescending(m => m.CreatedTime)
                    .Select(m => new MenuSelectionItem
                    {
                        MenuId = m.MenuId,
                        MenuName = m.MenuName,
                        IsActive = m.IsActive,
                        CreatedTime = m.CreatedTime
                    }).ToList();

                // Determine which menu to show
                Guid selectedMenuId;
                if (menuId.HasValue && menuId.Value != Guid.Empty)
                {
                    // User selected a specific menu
                    selectedMenuId = menuId.Value;
                    _logger.LogInformation("Kullanıcı tarafından seçilen menü önizlemesi yükleniyor. MenuId: {MenuId}", selectedMenuId);
                }
                else
                {
                    // Default to active menu, or first menu if no active
                    var activeMenu = availableMenus.FirstOrDefault(m => m.IsActive);
                    selectedMenuId = activeMenu?.MenuId ?? availableMenus.First().MenuId;
                    _logger.LogInformation("Varsayılan menü önizlemesi yükleniyor. MenuId: {MenuId}", selectedMenuId);
                }

                // Validate selected menu exists
                if (!availableMenus.Any(m => m.MenuId == selectedMenuId))
                {
                    TempData["ErrorMessage"] = "Seçilen menü bulunamadı.";
                    selectedMenuId = availableMenus.First().MenuId;
                }

                // Get the selected menu details
                var selectedMenuResult = await _menuService.GetByIdAsync(selectedMenuId);

                if (!selectedMenuResult.IsSuccess || selectedMenuResult.Data == null)
                {
                    TempData["ErrorMessage"] = "Menü detayları yüklenemedi.";
                    return View(new MenuPreviewViewModel
                    {
                        CafeName = cafeName,
                        MenuName = "Hata",
                        AvailableMenus = availableMenus,
                        SelectedMenuId = selectedMenuId
                    });
                }

                var selectedMenu = selectedMenuResult.Data;

                // Get categories for the selected menu
                var categoriesResult = await _menuCategoryService.GetAllAsyncByMenuId(selectedMenu.MenuId);

                if (!categoriesResult.IsSuccess || categoriesResult.Data == null || !categoriesResult.Data.Any())
                {
                    TempData["WarningMessage"] = "Bu menüde henüz kategori bulunmuyor.";
                    return View(new MenuPreviewViewModel
                    {
                        CafeName = cafeName,
                        MenuName = selectedMenu.MenuName,
                        AvailableMenus = availableMenus,
                        SelectedMenuId = selectedMenuId
                    });
                }

                // Get menu items for the categories
                var categoryIds = categoriesResult.Data.Select(c => c.MenuCategoryId).ToList();
                var itemsResult = await _menuItemService.GetAllAsyncByCategoryIds(categoryIds);

                var viewModel = new MenuPreviewViewModel
                {
                    CafeName = cafeName,
                    MenuName = selectedMenu.MenuName,
                    SelectedMenuId = selectedMenuId,
                    AvailableMenus = availableMenus,
                    Categories = categoriesResult.Data
                        .OrderBy(c => c.SortOrder)
                        .Select(c => new MenuPreviewCategoryVM
                        {
                            CategoryId = c.MenuCategoryId,
                            CategoryName = c.MenuCategoryName,
                            Description = c.Description,
                            ImageBase64 = c.ImageFileBytes != null && c.ImageFileBytes.Length > 0
                                ? Convert.ToBase64String(c.ImageFileBytes)
                                : null,
                            Items = itemsResult.IsSuccess && itemsResult.Data != null
                                ? itemsResult.Data
                                    .Where(i => i.MenuCategoryId == c.MenuCategoryId)
                                    .OrderBy(i => i.SortOrder)
                                    .Select(i => new MenuPreviewItemVM
                                    {
                                        ItemId = i.MenuItemId,
                                        ItemName = i.MenuItemName,
                                        Description = i.Description,
                                        Price = i.Price,
                                        ImageBase64 = i.ImageFileBytes != null && i.ImageFileBytes.Length > 0
                                            ? Convert.ToBase64String(i.ImageFileBytes)
                                            : null
                                    }).ToList()
                                : new List<MenuPreviewItemVM>()
                        }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Menü önizlemesi yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Menü önizlemesi yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}