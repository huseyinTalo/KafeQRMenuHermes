using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.UI.Areas.Admin.Models.MenuPreviewVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KafeQRMenu.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MenuPreviewController : Controller
    {
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly IMenuItemService _menuItemService;
        private readonly ILogger<MenuPreviewController> _logger;

        public MenuPreviewController(
            IMenuCategoryService menuCategoryService,
            IMenuItemService menuItemService,
            ILogger<MenuPreviewController> logger)
        {
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

                // Get all categories for this cafe
                var categoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(cafeId);
                if (!categoriesResult.IsSuccess || categoriesResult.Data == null)
                {
                    TempData["WarningMessage"] = "Henüz kategori eklenmemiş.";
                    return View(new MenuPreviewViewModel());
                }

                // Get all menu items for this cafe
                var itemsResult = await _menuItemService.GetAllAsyncCafesCatsItems(cafeId);

                var viewModel = new MenuPreviewViewModel
                {
                    CafeName = User.FindFirst("CafeName")?.Value ?? "Menü",
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