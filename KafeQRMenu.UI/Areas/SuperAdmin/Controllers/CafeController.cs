using KafeQRMenu.BLogic.DTOs.CafeDTOs;
using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.UI.Areas.SuperAdmin.Models.CafeVMs;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KafeQRMenu.UI.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class CafeController : Controller
    {
        private readonly ICafeService _cafeService;

        public CafeController(ICafeService cafeService)
        {
            _cafeService = cafeService;
        }

        // GET: SuperAdmin/Cafe
        public async Task<IActionResult> Index()
        {
            var result = await _cafeService.GetAllAsync();

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(new List<CafeListDTO>());
            }

            return View(result.Data.Adapt<List<SACafeListVM>>());
        }

        // GET: SuperAdmin/Cafe/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _cafeService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data.Adapt<SACafeVM>());
        }

        // GET: SuperAdmin/Cafe/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SuperAdmin/Cafe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SACafeCreateVM saCafeCreateVM)
        {
            if (!ModelState.IsValid)
            {
                return View(saCafeCreateVM);
            }

            byte[] imageData = null;

            // Resim yüklenmişse byte array'e çevir
            if (saCafeCreateVM.ImageFile != null && saCafeCreateVM.ImageFile.Length > 0)
            {
                // Resim boyut kontrolü (örn: max 5MB)
                if (saCafeCreateVM.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Resim boyutu 5MB'dan küçük olmalıdır.");
                    return View(saCafeCreateVM);
                }

                // Resim formatı kontrolü
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(saCafeCreateVM.ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Sadece .jpg, .jpeg, .png, .gif ve .webp formatları desteklenir.");
                    return View(saCafeCreateVM);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await saCafeCreateVM.ImageFile.CopyToAsync(memoryStream);
                    imageData = memoryStream.ToArray();
                }
            }

            var result = await _cafeService.CreateAsync(saCafeCreateVM.Adapt<CafeCreateDTO>(), imageData);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;
            return View(saCafeCreateVM);
        }

        // GET: SuperAdmin/Cafe/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _cafeService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var updateVM = result.Data.Adapt<SACafeUpdateVM>();

            return View(updateVM);
        }

        // POST: SuperAdmin/Cafe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SACafeUpdateVM saCafeUpdateVM)
        {
            if (id != saCafeUpdateVM.Id)
            {
                TempData["ErrorMessage"] = "Id uyuşmazlığı.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(saCafeUpdateVM);
            }

            byte[] imageData = null;

            // Yeni resim yüklenmişse
            if (saCafeUpdateVM.ImageFile != null && saCafeUpdateVM.ImageFile.Length > 0)
            {
                // Resim boyut kontrolü (örn: max 5MB)
                if (saCafeUpdateVM.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Resim boyutu 5MB'dan küçük olmalıdır.");
                    return View(saCafeUpdateVM);
                }

                // Resim formatı kontrolü
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(saCafeUpdateVM.ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Sadece .jpg, .jpeg, .png, .gif ve .webp formatları desteklenir.");
                    return View(saCafeUpdateVM);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await saCafeUpdateVM.ImageFile.CopyToAsync(memoryStream);
                    imageData = memoryStream.ToArray();
                }
            }

            var result = await _cafeService.UpdateAsync(saCafeUpdateVM.Adapt<CafeUpdateDTO>(), imageData);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;
            return View(saCafeUpdateVM);
        }

        // GET: SuperAdmin/Cafe/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz Id.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _cafeService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data.Adapt<SACafeVM>());
        }

        // POST: SuperAdmin/Cafe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _cafeService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var deleteResult = await _cafeService.DeleteAsync(result.Data);

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
    }
}