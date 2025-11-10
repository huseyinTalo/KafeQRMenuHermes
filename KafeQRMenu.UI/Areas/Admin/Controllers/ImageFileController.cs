using KafeQRMenu.BLogic.DTOs.ImageFileDTOs;
using KafeQRMenu.BLogic.Services.ImageServices;
using KafeQRMenu.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KafeQRMenu.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ImageFileController : Controller
    {
        private readonly IImageFileService _imageFileService;
        private readonly ILogger<ImageFileController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageFileController(
            IImageFileService imageFileService,
            ILogger<ImageFileController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _imageFileService = imageFileService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/ImageFile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var result = await _imageFileService.GetAllAsyncCached();

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(new List<ImageFileListDTO>());
                }

                ViewData["BreadcrumbTitle"] = "Resimler";
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resimler listelenirken hata oluştu");
                TempData["ErrorMessage"] = "Resimler yüklenirken bir hata oluştu.";
                return View(new List<ImageFileListDTO>());
            }
        }

        // GET: Admin/ImageFile/Upload
        [HttpGet]
        public IActionResult Upload()
        {
            ViewData["BreadcrumbTitle"] = "Resim Yükle";
            ViewBag.ImageContentTypes = GetImageContentTypes();
            return View(new ImageFileCreateDTO());
        }

        // POST: Admin/ImageFile/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, ImageContentType imageContentType, Guid? relatedEntityId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                ViewBag.ImageContentTypes = GetImageContentTypes();
                return View();
            }

            // Dosya boyutu kontrolü (5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "Dosya boyutu 5MB'dan büyük olamaz.";
                ViewBag.ImageContentTypes = GetImageContentTypes();
                return View();
            }

            // Dosya uzantısı kontrolü
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Sadece JPG, PNG, GIF ve WEBP dosyaları yüklenebilir.";
                ViewBag.ImageContentTypes = GetImageContentTypes();
                return View();
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var createDto = new ImageFileCreateDTO
                {
                    ImageByteFile = imageBytes,
                    IsActive = true,
                    ImageContentType = imageContentType
                };

                // İlişkili entity'yi ayarla
                switch (imageContentType)
                {
                    case ImageContentType.Category:
                        createDto.MenuCategoryId = relatedEntityId;
                        break;
                    case ImageContentType.Product:
                        createDto.MenuItemId = relatedEntityId;
                        break;
                    case ImageContentType.Cafe:
                    case ImageContentType.Background:
                        createDto.CafeId = relatedEntityId ?? GetCurrentUserCafeId();
                        break;
                    case ImageContentType.Person:
                        if (User.IsInRole("Admin"))
                            createDto.AdminId = relatedEntityId;
                        else
                            createDto.SuperAdminId = relatedEntityId;
                        break;
                }

                var result = await _imageFileService.CreateAsync(createDto);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Resim başarıyla yüklendi.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.Message;
                ViewBag.ImageContentTypes = GetImageContentTypes();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resim yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Resim yüklenirken bir hata oluştu.";
                ViewBag.ImageContentTypes = GetImageContentTypes();
                return View();
            }
        }

        // GET: Admin/ImageFile/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz resim ID'si.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _imageFileService.GetByIdAsyncCached(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = result.Message ?? "Resim bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["BreadcrumbTitle"] = "Resim Sil";
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resim silme sayfası yüklenirken hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/ImageFile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Geçersiz resim ID'si.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var imageDto = new ImageFileDTO { ImageId = id };
                var result = await _imageFileService.DeleteAsync(imageDto);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resim silinirken hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Resim silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/ImageFile/GetImage/5
        [HttpGet]
        [AllowAnonymous] // Resimlerin görüntülenmesi için
        public async Task<IActionResult> GettImage(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                var result = await _imageFileService.GetByIdAsyncCached(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound();
                }

                // Content type'ı belirle
                string contentType = result.Data.ImageContentType switch
                {
                    ImageContentType.Category or ImageContentType.Product or
                    ImageContentType.Cafe or ImageContentType.Background or
                    ImageContentType.Person => "image/jpeg",
                    _ => "image/jpeg"
                };

                return File(result.Data.ImageByteFile, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resim getirilirken hata oluştu. ID: {Id}", id);
                return NotFound();
            }
        }

        // AJAX: Upload for entity
        [HttpPost]
        public async Task<IActionResult> UploadForEntity(IFormFile file, ImageContentType contentType, Guid entityId)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Dosya seçilmedi." });
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var createDto = new ImageFileCreateDTO
                {
                    ImageByteFile = memoryStream.ToArray(),
                    IsActive = true,
                    ImageContentType = contentType
                };

                // Entity'ye göre ilişkiyi ayarla
                switch (contentType)
                {
                    case ImageContentType.Category:
                        createDto.MenuCategoryId = entityId;
                        break;
                    case ImageContentType.Product:
                        createDto.MenuItemId = entityId;
                        break;
                }

                var result = await _imageFileService.CreateAsync(createDto);

                if (result.IsSuccess)
                {
                    // Yeni oluşturulan resmin ID'sini döndür
                    return Json(new { success = true, message = "Resim yüklendi." });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AJAX resim yüklemesinde hata");
                return Json(new { success = false, message = "Resim yüklenirken hata oluştu." });
            }
        }

        #region Helper Methods

        private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetImageContentTypes()
        {
            return Enum.GetValues<ImageContentType>()
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e switch
                    {
                        ImageContentType.Category => "Kategori",
                        ImageContentType.Product => "Ürün",
                        ImageContentType.Cafe => "Kafe",
                        ImageContentType.Background => "Arkaplan",
                        ImageContentType.Person => "Kişi",
                        ImageContentType.Misc => "Diğer",
                        _ => e.ToString()
                    }
                }).ToList();
        }

        private Guid? GetCurrentUserCafeId()
        {
            var cafeIdClaim = User.FindFirst("CafeId")?.Value;
            if (Guid.TryParse(cafeIdClaim, out var cafeId))
            {
                return cafeId;
            }
            return null;
        }

        #endregion


        [HttpGet]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetImage(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return NotFound();
                }

                var result = await _imageFileService.GetByIdAsyncCached(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound();
                }

                return File(result.Data.ImageByteFile, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image {ImageId}", id);
                return NotFound();
            }
        }

        [HttpGet]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetThumbnail(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return NotFound();
                }

                var result = await _imageFileService.GetByIdAsyncCached(id);

                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound();
                }

                return File(result.Data.ImageByteFile, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving thumbnail {ImageId}", id);
                return NotFound();
            }
        }
    }
}