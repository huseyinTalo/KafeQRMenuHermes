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

namespace KafeQRMenu.UI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICafeService _cafeService;
    private readonly IMenuCategoryService _menuCategoryService;
    private readonly IMenuItemService _menuItemService;

    public HomeController(
        ILogger<HomeController> logger,
        ICafeService cafeService,
        IMenuCategoryService menuCategoryService,
        IMenuItemService menuItemService)
    {
        _logger = logger;
        _cafeService = cafeService;
        _menuCategoryService = menuCategoryService;
        _menuItemService = menuItemService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var menuVM = new MenuVM();

            // Tüm kafeleri getir ve ilkini al
            var cafesResult = await _cafeService.GetAllAsync();
            if (cafesResult.IsSuccess && cafesResult.Data != null && cafesResult.Data.Any())
            {
                var firstCafe = cafesResult.Data.First();
                menuVM.Cafe = firstCafe.Adapt<CafeVM>();
            }
            else
            {
                _logger.LogWarning("Hiç kafe bulunamadı.");
                menuVM.Cafe = new CafeVM
                {
                    CafeName = "Hoş Geldiniz",
                    Description = "Henüz bir kafe eklenmemiş."
                };
            }

            // Tüm kategorileri getir
            var categoriesResult = await _menuCategoryService.GetAllAsync();
            if (categoriesResult.IsSuccess && categoriesResult.Data != null)
            {
                menuVM.Categories = categoriesResult.Data.Adapt<List<MenuCategoryListVM>>();
            }
            else
            {
                menuVM.Categories = new List<MenuCategoryListVM>();
            }

            // Tüm ürünleri getir
            var productsResult = await _menuItemService.GetAllAsync();
            if (productsResult.IsSuccess && productsResult.Data != null)
            {
                menuVM.Products = productsResult.Data.Adapt<List<MenuItemListVM>>();
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
                Cafe = new CafeVM { CafeName = "Hata", Description = "Menü yüklenirken bir hata oluştu." },
                Categories = new List<MenuCategoryListVM>(),
                Products = new List<MenuItemListVM>()
            });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}