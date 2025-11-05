using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.UI.Areas.SuperAdmin.Models.MenuCategoryVMs;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KafeQRMenu.UI.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class MenuCategoryController : Controller
    {
        private readonly IMenuCategoryService _menuCategoryService;
        private readonly ICafeService _cafeService;

        public MenuCategoryController(
            IMenuCategoryService menuCategoryService,
            ICafeService cafeService)
        {
            _menuCategoryService = menuCategoryService;
            _cafeService = cafeService;
        }

        // GET: SuperAdmin/MenuCategory
        public async Task<IActionResult> Index()
        {
            var result = await _menuCategoryService.GetAllAsync();

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new List<SAMenuCategoryListVM>());
            }

            return View(result.Data.Adapt<List<SAMenuCategoryListVM>>());
        }

        // GET: SuperAdmin/MenuCategory/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _menuCategoryService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data.Adapt<SAMenuCategoryVM>());
        }

        // GET: SuperAdmin/MenuCategory/Create
        public async Task<IActionResult> Create()
        {
            await LoadCafesAsync();
            return View();
        }

        // POST: SuperAdmin/MenuCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SAMenuCategoryCreateVM viewModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadCafesAsync();
                return View(viewModel);
            }

            var result = await _menuCategoryService.CreateAsync(viewModel.Adapt<MenuCategoryCreateDTO>());

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;
            await LoadCafesAsync();
            return View(viewModel);
        }

        // GET: SuperAdmin/MenuCategory/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _menuCategoryService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var updateVM = result.Data.Adapt<SAMenuCategoryUpdateVM>();
            await LoadCafesAsync();
            return View(updateVM);
        }

        // POST: SuperAdmin/MenuCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SAMenuCategoryUpdateVM viewModel)
        {
            if (id != viewModel.MenuCategoryId)
            {
                TempData["ErrorMessage"] = "Id uyuşmazlığı.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadCafesAsync();
                return View(viewModel);
            }

            var result = await _menuCategoryService.UpdateAsync(viewModel.Adapt<MenuCategoryUpdateDTO>());

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;
            await LoadCafesAsync();
            return View(viewModel);
        }

        // GET: SuperAdmin/MenuCategory/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _menuCategoryService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data.Adapt<SAMenuCategoryVM>());
        }

        // POST: SuperAdmin/MenuCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _menuCategoryService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var deleteResult = await _menuCategoryService.DeleteAsync(result.Data);

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

        // Helper method to load cafes for dropdown
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