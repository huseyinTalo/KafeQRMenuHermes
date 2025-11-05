using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.UI.Areas.SuperAdmin.Models.MenuPreviewVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KafeQRMenu.UI.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class MenuPreviewController : Controller
    {
        private readonly ICafeService _cafeService;
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly IMenuItemService _menuItemService;
        private readonly ILogger<MenuPreviewController> _logger;

        public MenuPreviewController(
            ICafeService cafeService,
            IMenuCategoryService menuCategoryService,
            IMenuItemService menuItemService,
            ILogger<MenuPreviewController> logger)
        {
            _cafeService = cafeService;
            _menuCategoryService = menuCategoryService;
            _menuItemService = menuItemService;
            _logger = logger;
        }

        // GET: SuperAdmin/MenuPreview
        public async Task<IActionResult> Index()
        {
            await LoadCafesAsync();
            return View(new MenuPreviewVM());
        }

        // POST: SuperAdmin/MenuPreview/GetMenu
        [HttpPost]
        public async Task<IActionResult> GetMenu([FromBody] MenuPreviewVM model)
        {
            try
            {
                var parsedCafeId = model.cafeId;
                if (parsedCafeId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Lütfen bir cafe seçiniz." });
                }

                // Get cafe details
                var cafeResult = await _cafeService.GetByIdAsync(parsedCafeId);
                if (!cafeResult.IsSuccess)
                {
                    return Json(new { success = false, message = cafeResult.Message });
                }

                // Get all categories for this cafe
                var categoriesResult = await _menuCategoryService.GetAllAsync();
                if (!categoriesResult.IsSuccess)
                {
                    return Json(new { success = false, message = "Kategoriler yüklenemedi." });
                }

                var cafeCategories = categoriesResult.Data
                    .Where(c => c.CafeId == parsedCafeId)
                    .OrderBy(c => c.SortOrder)
                    .ToList();

                // Get all menu items
                var itemsResult = await _menuItemService.GetAllAsync();
                if (!itemsResult.IsSuccess)
                {
                    return Json(new { success = false, message = "Ürünler yüklenemedi." });
                }

                // Group items by category
                var menuData = cafeCategories.Select(category => new
                {
                    categoryId = category.MenuCategoryId,
                    categoryName = category.MenuCategoryName,
                    description = category.Description,
                    sortOrder = category.SortOrder,
                    items = itemsResult.Data
                        .Where(i => i.MenuCategoryId == category.MenuCategoryId)
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new
                        {
                            itemId = i.MenuItemId,
                            itemName = i.MenuItemName,
                            description = i.Description,
                            price = i.Price,
                            sortOrder = i.SortOrder
                        })
                        .ToList()
                }).ToList();

                return Json(new
                {
                    success = true,
                    cafeName = cafeResult.Data.CafeName,
                    cafeAddress = cafeResult.Data.Address,
                    categories = menuData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMenu metodunda hata oluştu. CafeId: {CafeId}", model.cafeId);
                return Json(new { success = false, message = "Bir hata oluştu." });
            }
        }

        private async Task LoadCafesAsync()
        {
            var cafesResult = await _cafeService.GetAllAsync();

            if (cafesResult.IsSuccess && cafesResult.Data != null)
            {
                ViewBag.Cafes = new SelectList(cafesResult.Data, "Id", "CafeName");
            }
            else
            {
                ViewBag.Cafes = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }
}