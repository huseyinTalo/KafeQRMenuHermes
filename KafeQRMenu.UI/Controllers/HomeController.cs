using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KafeQRMenu.UI.Models;
using KafeQRMenu.UI.Models.HomeVMs;
using KafeQRMenu.UI.Models.CafeVMs;
using KafeQRMenu.UI.Models.MenuCategoryVMs;
using KafeQRMenu.UI.Models.MenuItemVMs;
using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using Mapster;
using KafeQRMenu.BLogic.Services.MenuService;

namespace KafeQRMenu.UI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICafeService _cafeService;
    private readonly IMenuService _menuService;
    private readonly IMenuCategoryService _menuCategoryService;
    private readonly IMenuItemService _menuItemService;

    public HomeController(
        ILogger<HomeController> logger,
        ICafeService cafeService,
        IMenuService menuService,
        IMenuCategoryService menuCategoryService,
        IMenuItemService menuItemService)
    {
        _logger = logger;
        _cafeService = cafeService;
        _menuService = menuService;
        _menuCategoryService = menuCategoryService;
        _menuItemService = menuItemService;
    }

    public async Task<IActionResult> Index(Guid? cafeId = null)
    {
        try
        {
            var menuVM = new MenuVM();

            // Get cafe - either by ID or first available
            if (cafeId.HasValue && cafeId.Value != Guid.Empty)
            {
                var cafeResult = await _cafeService.GetByIdAsync(cafeId.Value);
                if (cafeResult.IsSuccess && cafeResult.Data != null)
                {
                    menuVM.Cafe = cafeResult.Data.Adapt<CafeVM>();
                }
            }
            else
            {
                var cafesResult = await _cafeService.GetAllAsync();
                if (cafesResult.IsSuccess && cafesResult.Data != null && cafesResult.Data.Any())
                {
                    var cafeNameClaim = User.FindFirst("CafeName")?.Value;
                    var adminLoggedCafe = cafesResult.Data.FirstOrDefault(x => x.CafeName == cafeNameClaim);

                    if (adminLoggedCafe != null)
                    {
                        menuVM.Cafe = adminLoggedCafe.Adapt<CafeVM>();
                        cafeId = adminLoggedCafe.Id;
                    }
                    else
                    {
                        // Fallback to PiGo cafe
                        var piGoCafe = cafesResult.Data.FirstOrDefault(x => x.CafeName == "PiGo");
                        if (piGoCafe != null)
                        {
                            menuVM.Cafe = piGoCafe.Adapt<CafeVM>();
                            cafeId = piGoCafe.Id;
                        }
                    }
                }
            }

            // If still no cafe found, show welcome message
            if (menuVM.Cafe == null || !cafeId.HasValue)
            {
                _logger.LogWarning("Hiç kafe bulunamadı.");
                menuVM.Cafe = new CafeVM
                {
                    CafeName = "Hoş Geldiniz",
                    Description = "Henüz bir kafe eklenmemiş."
                };
                menuVM.Categories = new List<MenuCategoryListVM>();
                menuVM.Products = new List<MenuItemListVM>();
                return View(menuVM);
            }

            // Get active menu for this cafe
            var activeMenuResult = await _menuService.GetActiveByCafeIdAsync(cafeId.Value);

            if (!activeMenuResult.IsSuccess || activeMenuResult.Data == null)
            {
                _logger.LogWarning("Kafe için aktif menü bulunamadı. CafeId: {CafeId}", cafeId.Value);
                menuVM.Cafe.Description = "Henüz aktif bir menü bulunmuyor.";
                menuVM.Categories = new List<MenuCategoryListVM>();
                menuVM.Products = new List<MenuItemListVM>();
                return View(menuVM);
            }

            var activeMenu = activeMenuResult.Data;
            _logger.LogInformation("Aktif menü bulundu. MenuId: {MenuId}, MenuName: {MenuName}",
                activeMenu.MenuId, activeMenu.MenuName);

            // Get categories for the active menu with images
            var categoriesResult = await _menuCategoryService.GetAllAsyncByMenuId(activeMenu.MenuId);
            if (categoriesResult.IsSuccess && categoriesResult.Data != null)
            {
                menuVM.Categories = categoriesResult.Data
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new MenuCategoryListVM
                    {
                        MenuCategoryId = c.MenuCategoryId,
                        MenuCategoryName = c.MenuCategoryName,
                        Description = c.Description,
                        SortOrder = c.SortOrder,
                        ImageFileId = c.ImageFileId,
                        ImageFileBase64 = c.ImageFileBytes != null && c.ImageFileBytes.Length > 0
                            ? Convert.ToBase64String(c.ImageFileBytes)
                            : null
                    })
                    .ToList();
            }
            else
            {
                menuVM.Categories = new List<MenuCategoryListVM>();
            }

            // Get menu items for the active menu's categories with images
            if (menuVM.Categories.Any())
            {
                var categoryIds = menuVM.Categories.Select(c => c.MenuCategoryId).ToList();
                var productsResult = await _menuItemService.GetAllAsyncByCategoryIds(categoryIds);

                if (productsResult.IsSuccess && productsResult.Data != null)
                {
                    menuVM.Products = productsResult.Data
                        .OrderBy(p => p.SortOrder)
                        .Select(p => new MenuItemListVM
                        {
                            MenuItemId = p.MenuItemId,
                            MenuItemName = p.MenuItemName,
                            Description = p.Description,
                            Price = p.Price,
                            SortOrder = p.SortOrder,
                            MenuCategoryId = p.MenuCategoryId,
                            MenuCategoryName = p.MenuCategoryName,
                            ImageFileId = p.ImageFileId,
                            ImageFileBase64 = p.ImageFileBytes != null && p.ImageFileBytes.Length > 0
                                ? Convert.ToBase64String(p.ImageFileBytes)
                                : null
                        })
                        .ToList();
                }
                else
                {
                    menuVM.Products = new List<MenuItemListVM>();
                }
            }
            else
            {
                menuVM.Products = new List<MenuItemListVM>();
            }

            return View(menuVM);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Index metodunda hata oluştu.");
            return View(new MenuVM
            {
                Cafe = new CafeVM
                {
                    CafeName = "Hata",
                    Description = "Menü yüklenirken bir hata oluştu."
                },
                Categories = new List<MenuCategoryListVM>(),
                Products = new List<MenuItemListVM>()
            });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}