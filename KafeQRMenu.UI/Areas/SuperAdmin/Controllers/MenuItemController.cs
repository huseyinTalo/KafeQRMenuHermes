using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.UI.Areas.SuperAdmin.Models.MenuItemVMs;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KafeQRMenu.UI.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class MenuItemController : Controller
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IMenuCategoryService _menuCategoryService;

        public MenuItemController(
            IMenuItemService menuItemService,
            IMenuCategoryService menuCategoryService)
        {
            _menuItemService = menuItemService;
            _menuCategoryService = menuCategoryService;
        }

        // GET: SuperAdmin/MenuItem
        public async Task<IActionResult> Index()
        {
            var result = await _menuItemService.GetAllAsync();

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new List<SAMenuItemListVM>());
            }

            return View(result.Data.Adapt<List<SAMenuItemListVM>>());
        }

        // GET: SuperAdmin/MenuItem/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _menuItemService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data.Adapt<SAMenuItemVM>());
        }

        // GET: SuperAdmin/MenuItem/Create
        public async Task<IActionResult> Create()
        {
            await LoadMenuCategoriesAsync();
            return View();
        }

        // POST: SuperAdmin/MenuItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SAMenuItemCreateVM viewModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadMenuCategoriesAsync();
                return View(viewModel);
            }

            var result = await _menuItemService.CreateAsync(viewModel.Adapt<MenuItemCreateDTO>());

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;
            await LoadMenuCategoriesAsync();
            return View(viewModel);
        }

        // GET: SuperAdmin/MenuItem/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _menuItemService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var updateVM = result.Data.Adapt<SAMenuItemUpdateVM>();
            await LoadMenuCategoriesAsync();
            return View(updateVM);
        }

        // POST: SuperAdmin/MenuItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SAMenuItemUpdateVM viewModel)
        {
            if (id != viewModel.MenuItemId)
            {
                TempData["ErrorMessage"] = "Id uyuşmazlığı.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadMenuCategoriesAsync();
                return View(viewModel);
            }

            var result = await _menuItemService.UpdateAsync(viewModel.Adapt<MenuItemUpdateDTO>());

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;
            await LoadMenuCategoriesAsync();
            return View(viewModel);
        }

        // GET: SuperAdmin/MenuItem/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _menuItemService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data.Adapt<SAMenuItemVM>());
        }

        // POST: SuperAdmin/MenuItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _menuItemService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var deleteResult = await _menuItemService.DeleteAsync(result.Data);

            if (deleteResult.IsSuccess)
            {
                TempData["SuccessMessage"] = deleteResult.Message;
            }
            else
            {
                TempData["ErrorMessage"] = deleteResult.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to load menu categories for dropdown
        private async Task LoadMenuCategoriesAsync()
        {
            var categoriesResult = await _menuCategoryService.GetAllAsync();

            if (categoriesResult.IsSuccess && categoriesResult.Data != null)
            {
                ViewBag.MenuCategories = new SelectList(categoriesResult.Data, "MenuCategoryId", "MenuCategoryName");
            }
            else
            {
                ViewBag.MenuCategories = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }
}