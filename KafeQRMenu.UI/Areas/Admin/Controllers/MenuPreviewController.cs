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

        // GET: Admin/MenuPreview
        [HttpGet]
        public async Task<IActionResult> Index()
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

                // Get active menu for this cafe
                var activeMenuResult = await _menuService.GetActiveByCafeIdAsync(cafeId);

                if (!activeMenuResult.IsSuccess || activeMenuResult.Data == null)
                {
                    TempData["WarningMessage"] = "Henüz aktif bir menü bulunmuyor. Lütfen bir menü oluşturun ve aktif hale getirin.";
                    return View(new MenuPreviewViewModel
                    {
                        CafeName = cafeName,
                        MenuName = "Aktif Menü Yok"
                    });
                }

                var activeMenu = activeMenuResult.Data;
                _logger.LogInformation("Aktif menü önizlemesi yükleniyor. MenuId: {MenuId}, MenuName: {MenuName}",
                    activeMenu.MenuId, activeMenu.MenuName);

                // Get categories for the active menu
                var categoriesResult = await _menuCategoryService.GetAllAsyncByMenuId(activeMenu.MenuId);

                if (!categoriesResult.IsSuccess || categoriesResult.Data == null || !categoriesResult.Data.Any())
                {
                    TempData["WarningMessage"] = "Aktif menüde henüz kategori bulunmuyor.";
                    return View(new MenuPreviewViewModel
                    {
                        CafeName = cafeName,
                        MenuName = activeMenu.MenuName
                    });
                }

                // Get menu items for the categories
                var categoryIds = categoriesResult.Data.Select(c => c.MenuCategoryId).ToList();
                var itemsResult = await _menuItemService.GetAllAsyncByCategoryIds(categoryIds);

                var viewModel = new MenuPreviewViewModel
                {
                    CafeName = cafeName,
                    MenuName = activeMenu.MenuName,
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