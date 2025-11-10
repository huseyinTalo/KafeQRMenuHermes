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
                    //TODO find a better logic for this for production
                    //we can hide the cafe ID or name or location or anything in urls but any changes in the db would mean changing all of the physical qr codes
                    //we can make the user choose but that would mean they would be exposed to unnecessary info
                    var cafeINameClaim = User.FindFirst("CafeName")?.Value;
                    var adminLoggedCafe = cafesResult.Data.FirstOrDefault(x => x.CafeName == cafeINameClaim);
                    menuVM.Cafe = adminLoggedCafe.Adapt<CafeVM>();
                    cafeId = adminLoggedCafe.Id;
                }
                else
                {
                    var cafesOfAll = await _cafeService.GetAllAsync();
                    foreach(var item in cafesOfAll.Data)
                    {
                        if(item.CafeName == "PiGo")
                        {
                            cafeId = item.Id;
                        }
                    }
                }
            }

            // If still no cafe found, show welcome message
            if (menuVM.Cafe == null)
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

            // Get categories for this cafe with images
            var categoriesResult = await _menuCategoryService.GetAllAsyncCafesCats(cafeId.Value);
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

            // Get menu items for this cafe with images
            var productsResult = await _menuItemService.GetAllAsyncCafesCatsItems(cafeId.Value);
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