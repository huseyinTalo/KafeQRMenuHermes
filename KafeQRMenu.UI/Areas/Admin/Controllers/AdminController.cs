using KafeQRMenu.BLogic.DTOs.CafeDTOs;
using KafeQRMenu.BLogic.Services.AdminServices;
using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.UI.Areas.Admin.Models.AdminVMs;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace KafeQRMenu.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ICafeService _cafeService;

        public AdminController(IAdminService adminService, ICafeService cafeService)
        {
            _adminService = adminService;
            _cafeService = cafeService;
        }

        #region Helper Methods

        private Guid GetUserCafeId()
        {
            var cafeIdClaim = User.FindFirst("CafeId")?.Value;

            if (string.IsNullOrEmpty(cafeIdClaim) || !Guid.TryParse(cafeIdClaim, out var cafeId))
            {
                return Guid.Empty;
            }

            return cafeId;
        }

        #endregion

        public async Task<IActionResult> Profile()
        {
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(identityId))
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _adminService.GetByIdentityIdAsync(identityId, false);

            if (!result.IsSuccess || result.Data is null)
            {
                TempData["Error"] = result.Message ?? "Profil bilgileri yüklenemedi.";
                return View(new AdminProfileVM());
            }

            var profileView = result.Data.Adapt<AdminProfileVM>();
            return View(profileView);
        }

        public async Task<IActionResult> CafeDetails()
        {
            var cafeId = GetUserCafeId();

            if (cafeId == Guid.Empty)
            {
                TempData["Error"] = "Kafe bilgisi bulunamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _cafeService.GetByIdAsync(cafeId, false);

            if (!result.IsSuccess || result.Data is null)
            {
                TempData["Error"] = result.Message ?? "Kafe bilgileri yüklenemedi.";
                return RedirectToAction(nameof(Profile));
            }

            var cafeView = result.Data.Adapt<AdminCafeVM>();
            return View(cafeView);
        }

        // GET: Admin/Admin/EditCafe
        public async Task<IActionResult> EditCafe()
        {
            var cafeId = GetUserCafeId();

            if (cafeId == Guid.Empty)
            {
                TempData["Error"] = "Kafe bilgisi bulunamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction(nameof(Profile));
            }

            var result = await _cafeService.GetByIdAsync(cafeId, false);

            if (!result.IsSuccess || result.Data is null)
            {
                TempData["Error"] = result.Message ?? "Kafe bilgileri yüklenemedi.";
                return RedirectToAction(nameof(CafeDetails));
            }

            var editVM = result.Data.Adapt<AdminCafeEditVM>();
            return View(editVM);
        }

        // POST: Admin/Admin/EditCafe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCafe(AdminCafeEditVM editVM)
        {
            var cafeId = GetUserCafeId();

            // Güvenlik kontrolü: Sadece kendi kafesini düzenleyebilir
            if (cafeId == Guid.Empty || cafeId != editVM.Id)
            {
                TempData["Error"] = "Bu kafeyi düzenleme yetkiniz yok.";
                return RedirectToAction(nameof(CafeDetails));
            }

            if (!ModelState.IsValid)
            {
                return View(editVM);
            }

            byte[]? imageData = null;

            // Yeni resim yüklenmişse
            if (editVM.ImageFile != null && editVM.ImageFile.Length > 0)
            {
                // Resim boyut kontrolü (max 5MB)
                if (editVM.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Logo boyutu 5MB'dan küçük olmalıdır.");
                    return View(editVM);
                }

                // Resim formatı kontrolü
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(editVM.ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Sadece .jpg, .jpeg, .png, .gif ve .webp formatları desteklenir.");
                    return View(editVM);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await editVM.ImageFile.CopyToAsync(memoryStream);
                    imageData = memoryStream.ToArray();
                }
            }

            // Mevcut kafe bilgilerini al (DomainName gibi değiştirilmeyecek alanlar için)
            var existingCafe = await _cafeService.GetByIdAsync(cafeId, false);

            if (!existingCafe.IsSuccess || existingCafe.Data is null)
            {
                TempData["Error"] = "Kafe bilgileri alınamadı.";
                return View(editVM);
            }

            // Update DTO oluştur
            var updateDto = new CafeUpdateDTO
            {
                Id = editVM.Id,
                CafeName = editVM.CafeName,
                Description = editVM.Description,
                Address = editVM.Address,
                DomainName = existingCafe.Data.DomainName, // DomainName değiştirilemiyor
                ImageFileId = editVM.ImageFileId
            };

            var result = await _cafeService.UpdateAsync(updateDto, imageData);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Kafe bilgileri başarıyla güncellendi.";
                return RedirectToAction(nameof(CafeDetails));
            }

            TempData["Error"] = result.Message;
            return View(editVM);
        }
    }
}