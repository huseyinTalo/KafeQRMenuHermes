using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KafeQRMenu.UI.Areas.Admin.Models.DashboardVMs;
using KafeQRMenu.BLogic.Services.CafeServices;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using KafeQRMenu.BLogic.Services.AdminServices;
using Microsoft.IdentityModel.Tokens;

namespace KafeQRMenu.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly IMenuItemService _menuItemService;
        private readonly ICafeService _cafeService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAdminService _adminService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IMenuCategoryService menuCategoryService,
            IMenuItemService menuItemService,
            ILogger<HomeController> logger,
            ICafeService cafeService,
            UserManager<IdentityUser> userManager,
            IAdminService adminService)
        {
            _menuCategoryService = menuCategoryService;
            _menuItemService = menuItemService;
            _logger = logger;
            _cafeService = cafeService;
            _userManager = userManager;
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new DashboardViewModel
                {
                    AdminName = User.Identity?.Name ?? "Admin",
                    CafeName = await GetCafeName()
                };

                // Get categories
                var categoriesResult = await _menuCategoryService.GetAllAsync();
                if (categoriesResult.IsSuccess && categoriesResult.Data != null)
                {
                    var categories = categoriesResult.Data;
                    viewModel.TotalCategories = categories.Count;

                    // This month's categories
                    var thisMonth = DateTime.Now.Month;
                    viewModel.CategoriesThisMonth = categories
                        .Count(c => c.CreatedTime.Month == thisMonth);

                    // Recent categories (last 5)
                    viewModel.RecentCategories = categories
                        .OrderByDescending(c => c.CreatedTime)
                        .Take(5)
                        .Select(c => new RecentCategoryDTO
                        {
                            Id = c.MenuCategoryId,
                            MenuCategoryName = c.MenuCategoryName,
                            CreatedDate = c.CreatedTime,
                            ItemCount = 0 // Will be updated below
                        })
                        .ToList();

                    // Chart data
                    viewModel.CategoryNames = categories
                        .OrderBy(c => c.SortOrder)
                        .Take(10)
                        .Select(c => c.MenuCategoryName)
                        .ToList();
                }

                // Get menu items
                var menuItemsResult = await _menuItemService.GetAllAsync();
                if (menuItemsResult.IsSuccess && menuItemsResult.Data != null)
                {
                    var menuItems = menuItemsResult.Data;
                    viewModel.TotalMenuItems = menuItems.Count;

                    // This month's items
                    var thisMonth = DateTime.Now.Month;
                    viewModel.MenuItemsThisMonth = menuItems
                        .Count(m => m.CreatedTime.Month == thisMonth);

                    // Average price
                    if (menuItems.Any())
                    {
                        viewModel.AveragePrice = menuItems.Average(m => m.Price);
                    }

                    // Recent menu items (last 5)
                    viewModel.RecentMenuItems = menuItems
                        .OrderByDescending(m => m.CreatedTime)
                        .Take(5)
                        .Select(m => new RecentMenuItemDTO
                        {
                            Id = m.MenuItemId,
                            MenuItemName = m.MenuItemName,
                            CategoryName = m.MenuCategoryName ?? "Kategorisiz",
                            Price = m.Price,
                            CreatedDate = m.CreatedTime
                        })
                        .ToList();

                    // Update category item counts
                    var categoryItemCounts = menuItems
                        .GroupBy(m => m.MenuCategoryId)
                        .ToDictionary(g => g.Key, g => g.Count());

                    foreach (var category in viewModel.RecentCategories)
                    {
                        if (categoryItemCounts.TryGetValue(category.Id, out int count))
                        {
                            category.ItemCount = count;
                        }
                    }

                    // Chart data - item counts per category
                    viewModel.CategoryItemCounts = viewModel.CategoryNames
                        .Select(name =>
                        {
                            var categoryId = categoriesResult.Data
                                .FirstOrDefault(c => c.MenuCategoryName == name)?.MenuCategoryId;

                            if (categoryId.HasValue && categoryItemCounts.TryGetValue(categoryId.Value, out int count))
                            {
                                return count;
                            }
                            return 0;
                        })
                        .ToList();
                }

                // QR Views (mock data for now - implement when QR tracking is ready)
                viewModel.QRViewsToday = new Random().Next(50, 200);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Dashboard yüklenirken bir hata oluştu.";

                return View(new DashboardViewModel
                {
                    AdminName = User.Identity?.Name ?? "Admin",
                    CafeName = await GetCafeName()
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestStats()
        {
            try
            {
                var categoriesResult = await _menuCategoryService.GetAllAsync();
                var menuItemsResult = await _menuItemService.GetAllAsync();

                var stats = new
                {
                    totalCategories = categoriesResult.Data?.Count ?? 0,
                    totalMenuItems = menuItemsResult.Data?.Count ?? 0,
                    qrViewsToday = new Random().Next(50, 200)
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstatistikler güncellenirken hata");
                return Json(new { error = true });
            }
        }

        private async Task<string> GetCafeName()
        {
            // Session veya Claim'den cafe adını al
            var userClaim = User.Claims.FirstOrDefault(x => x.Type == "CafeName");
                return userClaim.Value;
        }

        private Guid GetCurrentCafeId()
        {
            var cafeIdClaim = User.FindFirst("CafeId")?.Value;
            if (Guid.TryParse(cafeIdClaim, out Guid cafeId))
            {
                return cafeId;
            }

            var sessionCafeId = HttpContext.Session.GetString("CafeId");
            if (Guid.TryParse(sessionCafeId, out Guid sessionCafe))
            {
                return sessionCafe;
            }

            return Guid.Empty;
        }

        private Guid GetCurrentAdminId()
        {
            var adminIdClaim = User.FindFirst("AdminId")?.Value;
            if (Guid.TryParse(adminIdClaim, out Guid adminId))
            {
                return adminId;
            }
            return Guid.Empty;
        }
    }
}