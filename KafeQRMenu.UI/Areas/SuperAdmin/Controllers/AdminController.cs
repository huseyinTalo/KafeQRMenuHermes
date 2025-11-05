using KafeQRMenu.BLogic.DTOs.AdminDTOs;
using KafeQRMenu.BLogic.Services.AdminServices;
using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.UI.Areas.SuperAdmin.Models.AdminViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KafeQRMenu.UI.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles ="SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ICafeService _cafeService;

        public AdminController(IAdminService adminService, ICafeService cafeService)
        {
            _adminService = adminService;
            _cafeService = cafeService;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _adminService.GetAllAsync();

            if(!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(result.Data.Adapt<List<SAAdminListVM>>());
            }

            TempData["Success"] = result.Message;
            return View(result.Data.Adapt<List<SAAdminListVM>>());

        }
        // GET: Admin/Index
        public async Task<IActionResult> Create()
        {
            var cafesResult = await _cafeService.GetAllAsync();
            if (!cafesResult.IsSuccess)
            {
                TempData["Error"] = cafesResult.Message;
                return View(new SAAdminCreateVM());
            }

            ViewBag.Cafes = new SelectList(cafesResult.Data, "Id", "CafeName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SAAdminCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                var cafesResult = await _cafeService.GetAllAsync();
                if (cafesResult.IsSuccess)
                {
                    ViewBag.Cafes = new SelectList(cafesResult.Data, "Id", "CafeName");
                }
                return View(model);
            }

            var createDto = model.Adapt<AdminCreateDTO>();
            var result = await _adminService.CreateAsync(createDto);

            if (result.IsSuccess)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Message;

            var cafesResult2 = await _cafeService.GetAllAsync();
            if (cafesResult2.IsSuccess)
            {
                ViewBag.Cafes = new SelectList(cafesResult2.Data, "Id", "CafeName");
            }

            return View(model);
        }

        // GET: Admin/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _adminService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data.Adapt<SAAdminVM>());
        }


        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _adminService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var cafesResult2 = await _cafeService.GetAllAsync();
            if (cafesResult2.IsSuccess)
            {
                ViewBag.Cafes = new SelectList(cafesResult2.Data, "Id", "CafeName");
            }

            var adminUpdateVM = result.Data.Adapt<SAAdminUpdateVM>();
            return View(adminUpdateVM);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SAAdminUpdateVM adminUpdateVM)
        {
            if (id != adminUpdateVM.Id)
            {
                TempData["Error"] = "ID uyuşmazlığı.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(adminUpdateVM);
            }

            var adminUpdateDto = adminUpdateVM.Adapt<AdminUpdateDTO>();
            var result = await _adminService.UpdateAsync(adminUpdateDto);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(adminUpdateVM);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _adminService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data.Adapt<SAAdminVM>());
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var getResult = await _adminService.GetByIdAsync(id);
            if (!getResult.IsSuccess)
            {
                TempData["Error"] = getResult.Message;
                return RedirectToAction(nameof(Index));
            }

            var adminDto = getResult.Data;
            var deleteResult = await _adminService.DeleteAsync(adminDto);

            if (!deleteResult.IsSuccess)
            {
                TempData["Error"] = deleteResult.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = deleteResult.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}