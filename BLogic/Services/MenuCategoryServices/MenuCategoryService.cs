using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.CafeRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
using KafeQRMenu.DataAccess.Repositories.ImageRepositories;
using Mapster;
using Microsoft.Extensions.Logging;
using KafeQRMenu.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using KafeQRMenu.BLogic.DTOs.ImageFileDTOs;


namespace KafeQRMenu.BLogic.Services.MenuCategoryServices
{
    public class MenuCategoryService : IMenuCategoryService
    {
        private readonly IMenuCategoryRepository _menuCategoryRepository;
        private readonly ILogger<MenuCategoryService> _logger;
        private readonly ICafeRepository _cafeRepository;
        private readonly IImageFileRepository _imageFileRepository;

        public MenuCategoryService(
            IMenuCategoryRepository menuCategoryRepository,
            ILogger<MenuCategoryService> logger,
            ICafeRepository cafeRepository,
            IImageFileRepository imageFileRepository)
        {
            _menuCategoryRepository = menuCategoryRepository;
            _logger = logger;
            _cafeRepository = cafeRepository;
            _imageFileRepository = imageFileRepository;
        }

        public async Task<IDataResult<MenuCategoryDTO>> CreateAsync(MenuCategoryCreateDTO menuCategoryCreateDto, byte[] imageData = null)
        {
            _logger.LogInformation("MenuCategory oluşturma işlemi başlatıldı. CategoryName: {CategoryName}", menuCategoryCreateDto.MenuCategoryName);

            try
            {
                if (menuCategoryCreateDto == null)
                {
                    _logger.LogWarning("MenuCategory oluşturulamadı. DTO boş olamaz.");
                    return new ErrorDataResult<MenuCategoryDTO>(new MenuCategoryDTO(), "Kategori bilgisi boş olamaz.");
                }

                // Validate Cafe exists
                _logger.LogDebug("Cafe kontrolü yapılıyor. CafeId: {CafeId}", menuCategoryCreateDto.CafeId);
                var cafeResult = await _cafeRepository.GetById(menuCategoryCreateDto.CafeId);
                if (cafeResult == null || cafeResult.Id == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz CafeId. CafeId: {CafeId}", menuCategoryCreateDto.CafeId);
                    return new ErrorDataResult<MenuCategoryDTO>(new MenuCategoryDTO(), "Kafe bilgisi bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor.");
                var strategy = await _menuCategoryRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IDataResult<MenuCategoryDTO>>(async () =>
                {
                    var transaction = await _menuCategoryRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        ImageFile createdImageFile = null;

                        // Create ImageFile if image data is provided
                        if (imageData != null && imageData.Length > 0)
                        {
                            _logger.LogDebug("ImageFile oluşturuluyor.");

                            createdImageFile = new ImageFile
                            {
                                ImageByteFile = imageData,
                                IsActive = true,
                                ImageContentType = ImageContentType.Category,
                                MenuCategoryId = null // Will be set after MenuCategory is created
                            };

                            await _imageFileRepository.AddAsync(createdImageFile);
                            var imageResult = await _imageFileRepository.SaveChangeAsync();

                            if (imageResult <= 0)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError("ImageFile oluşturulamadı.");
                                return new ErrorDataResult<MenuCategoryDTO>(new MenuCategoryDTO(), "Resim yüklenirken bir hata oluştu.");
                            }

                            _logger.LogInformation("ImageFile başarıyla oluşturuldu. ImageId: {ImageId}", createdImageFile.Id);
                            menuCategoryCreateDto.ImageFileId = createdImageFile.Id;
                        }

                        // Create MenuCategory
                        _logger.LogDebug("MenuCategory entity oluşturuluyor.");
                        var menuCategoryEntity = menuCategoryCreateDto.Adapt<MenuCategory>();
                        menuCategoryEntity.Cafe = cafeResult;
                        menuCategoryEntity.MenuCategoryImageId = menuCategoryCreateDto.ImageFileId;

                        await _menuCategoryRepository.AddAsync(menuCategoryEntity);
                        var categoryResult = await _menuCategoryRepository.SaveChangeAsync();

                        if (categoryResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("MenuCategory oluşturulamadı.");
                            return new ErrorDataResult<MenuCategoryDTO>(new MenuCategoryDTO(), "Kategori oluşturulurken bir hata oluştu.");
                        }

                        _logger.LogInformation("MenuCategory başarıyla oluşturuldu. CategoryId: {CategoryId}", menuCategoryEntity.Id);

                        // Update ImageFile with MenuCategoryId if image was created
                        if (createdImageFile != null)
                        {
                            _logger.LogDebug("ImageFile güncelleniyor. MenuCategoryId ekleniyor.");
                            createdImageFile.MenuCategoryId = menuCategoryEntity.Id;
                            await _imageFileRepository.UpdateAsync(createdImageFile);
                            var updateImageResult = await _imageFileRepository.SaveChangeAsync();

                            if (updateImageResult <= 0)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError("ImageFile MenuCategoryId ile güncellenemedi.");
                                return new ErrorDataResult<MenuCategoryDTO>(new MenuCategoryDTO(), "Resim kategori ile ilişkilendirilemedi.");
                            }

                            _logger.LogInformation("ImageFile MenuCategoryId ile güncellendi.");
                        }

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. MenuCategory başarıyla oluşturuldu. CategoryName: {CategoryName}",
                            menuCategoryCreateDto.MenuCategoryName);

                        var resultDto = menuCategoryEntity.Adapt<MenuCategoryDTO>();
                        return new SuccessDataResult<MenuCategoryDTO>(resultDto, "Kategori başarıyla oluşturuldu.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "MenuCategory oluşturma sırasında hata oluştu. Transaction rollback yapıldı. CategoryName: {CategoryName}",
                                menuCategoryCreateDto.MenuCategoryName);
                        }
                        else
                        {
                            _logger.LogError(ex, "MenuCategory oluşturma sırasında hata oluştu. CategoryName: {CategoryName}",
                                menuCategoryCreateDto.MenuCategoryName);
                        }

                        return new ErrorDataResult<MenuCategoryDTO>(new MenuCategoryDTO(), $"Bir hata oluştu: {ex.Message}");
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MenuCategory oluşturma işlemi başarısız oldu. CategoryName: {CategoryName}",
                    menuCategoryCreateDto.MenuCategoryName);
                return new ErrorDataResult<MenuCategoryDTO>(new MenuCategoryDTO(), $"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteAsync(MenuCategoryDTO menuCategoryDto)
        {
            _logger.LogInformation("MenuCategory silme işlemi başlatıldı. CategoryId: {CategoryId}", menuCategoryDto.MenuCategoryId);

            try
            {
                if (menuCategoryDto == null || menuCategoryDto.MenuCategoryId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz MenuCategory bilgisi.");
                    return new ErrorResult("Geçersiz Kategori bilgisi.");
                }

                // Get MenuCategory entity
                _logger.LogDebug("MenuCategory entity getiriliyor. CategoryId: {CategoryId}", menuCategoryDto.MenuCategoryId);
                var menuCategoryEntity = await _menuCategoryRepository.GetById(menuCategoryDto.MenuCategoryId);

                if (menuCategoryEntity == null)
                {
                    _logger.LogWarning("MenuCategory bulunamadı. CategoryId: {CategoryId}", menuCategoryDto.MenuCategoryId);
                    return new ErrorResult("Kategori bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor. CategoryId: {CategoryId}", menuCategoryDto.MenuCategoryId);
                var strategy = await _menuCategoryRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _menuCategoryRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        ImageFile imageFileToDelete = null;

                        // Get and delete associated ImageFile if exists
                        if (menuCategoryEntity.MenuCategoryImageId.HasValue)
                        {
                            _logger.LogDebug("İlişkili ImageFile getiriliyor. ImageId: {ImageId}", menuCategoryEntity.MenuCategoryImageId.Value);
                            imageFileToDelete = await _imageFileRepository.GetById(menuCategoryEntity.MenuCategoryImageId.Value);

                            if (imageFileToDelete != null)
                            {
                                _logger.LogDebug("ImageFile siliniyor. ImageId: {ImageId}", imageFileToDelete.Id);
                                await _imageFileRepository.DeleteAsync(imageFileToDelete);
                                var imageDeleteResult = await _imageFileRepository.SaveChangeAsync();

                                if (imageDeleteResult <= 0)
                                {
                                    await transaction.RollbackAsync();
                                    _logger.LogError("ImageFile silinemedi. ImageId: {ImageId}", imageFileToDelete.Id);
                                    return new ErrorResult("Kategori resmi silinirken bir hata oluştu.");
                                }

                                _logger.LogInformation("ImageFile başarıyla silindi. ImageId: {ImageId}", imageFileToDelete.Id);
                            }
                        }

                        // Delete MenuCategory
                        _logger.LogDebug("MenuCategory siliniyor. CategoryId: {CategoryId}", menuCategoryDto.MenuCategoryId);
                        await _menuCategoryRepository.DeleteAsync(menuCategoryEntity);
                        var categoryDeleteResult = await _menuCategoryRepository.SaveChangeAsync();

                        if (categoryDeleteResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("MenuCategory silinemedi. CategoryId: {CategoryId}", menuCategoryDto.MenuCategoryId);
                            return new ErrorResult("Kategori silinirken bir hata oluştu.");
                        }

                        _logger.LogInformation("MenuCategory başarıyla silindi. CategoryId: {CategoryId}", menuCategoryDto.MenuCategoryId);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. MenuCategory ve ilişkili ImageFile başarıyla silindi. CategoryId: {CategoryId}",
                            menuCategoryDto.MenuCategoryId);

                        return new SuccessResult("Kategori başarıyla silindi.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "MenuCategory silme sırasında hata oluştu. Transaction rollback yapıldı. CategoryId: {CategoryId}",
                                menuCategoryDto.MenuCategoryId);
                        }
                        else
                        {
                            _logger.LogError(ex, "MenuCategory silme sırasında hata oluştu. CategoryId: {CategoryId}",
                                menuCategoryDto.MenuCategoryId);
                        }

                        return new ErrorResult($"Bir hata oluştu: {ex.Message}");
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MenuCategory silme işlemi başarısız oldu. CategoryId: {CategoryId}", menuCategoryDto.MenuCategoryId);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IDataResult<List<MenuCategoryListDTO>>> GetAllAsync()
        {
            try
            {
                var menuCategories = await _menuCategoryRepository.GetAllAsync(
                    orderBy: x => x.CreatedTime,
                    orderByDescending: true
                );
                if (menuCategories == null || !menuCategories.Any())
                {
                    return new SuccessDataResult<List<MenuCategoryListDTO>>(
                        new List<MenuCategoryListDTO>(),
                        "Kayıt bulunamadı."
                    );
                }

                var menuCategoryDtoList = menuCategories.Adapt<List<MenuCategoryListDTO>>();

                return new SuccessDataResult<List<MenuCategoryListDTO>>(
                    menuCategoryDtoList,
                    $"{menuCategoryDtoList.Count} adet Kategori listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync metodunda hata oluştu.");
                return new ErrorDataResult<List<MenuCategoryListDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IDataResult<List<MenuCategoryListDTO>>> GetAllAsyncCafesCats(Guid CafeId)
        {
            try
            {
                var menuCategories = await _menuCategoryRepository.GetAllAsync(
                    x => x.CafeId == CafeId,
                    orderBy: x => x.CreatedTime,
                    orderByDescending: true
                );


                if (menuCategories == null || !menuCategories.Any())
                {
                    return new SuccessDataResult<List<MenuCategoryListDTO>>(
                        new List<MenuCategoryListDTO>(),
                        "Kayıt bulunamadı."
                    );
                }

                var menuCategoryDtoList = menuCategories.Adapt<List<MenuCategoryListDTO>>();
                foreach (var item in menuCategoryDtoList)
                {
                    var imageId = item.ImageFileId ?? Guid.Empty;

                    var imageByBytes = await _imageFileRepository.GetById(imageId);
                    if (imageByBytes is not null)
                    {
                        if (imageByBytes.ImageByteFile.Length != 0)
                        {
                            item.ImageFileBytes = imageByBytes.ImageByteFile;
                        }
                    }
                }
                return new SuccessDataResult<List<MenuCategoryListDTO>>(
                    menuCategoryDtoList,
                    $"{menuCategoryDtoList.Count} adet Kategori listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync metodunda hata oluştu.");
                return new ErrorDataResult<List<MenuCategoryListDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IDataResult<MenuCategoryDTO>> GetByIdAsync(Guid Id)
        {
            try
            {
                if (Id == Guid.Empty)
                {
                    return new ErrorDataResult<MenuCategoryDTO>(null, "Geçersiz Id.");
                }

                var menuCategoryEntity = await _menuCategoryRepository.GetById(Id);

                if (menuCategoryEntity == null)
                {
                    return new ErrorDataResult<MenuCategoryDTO>(null, "Kategori bulunamadı.");
                }

                var menuCategoryDto = menuCategoryEntity.Adapt<MenuCategoryDTO>();

                if (menuCategoryDto.ImageFileId is not null && menuCategoryDto.ImageFileId != Guid.Empty)
                {
                    var imageData = await _imageFileRepository.GetById(menuCategoryDto.ImageFileId ?? Guid.Empty);

                    if (imageData is not null)
                    {
                        if(imageData.ImageByteFile is not null)

    {
                            menuCategoryDto.ImageFileBytes = imageData.ImageByteFile; 
                        }

                    }
                }

                return new SuccessDataResult<MenuCategoryDTO>(menuCategoryDto, "Kategori detayları getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync metodunda hata oluştu. Id: {Id}", Id);
                return new ErrorDataResult<MenuCategoryDTO>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IResult> UpdateAsync(MenuCategoryUpdateDTO menuCategoryUpdateDto, byte[] newImageData = null)
        {
            _logger.LogInformation("MenuCategory güncelleme işlemi başlatıldı. CategoryId: {CategoryId}, CategoryName: {CategoryName}",
                menuCategoryUpdateDto.MenuCategoryId, menuCategoryUpdateDto.MenuCategoryName);

            try
            {
                if (menuCategoryUpdateDto == null || menuCategoryUpdateDto.MenuCategoryId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz kategori bilgisi.");
                    return new ErrorResult("Geçersiz kategori bilgisi.");
                }

                // Get existing MenuCategory
                _logger.LogDebug("Mevcut MenuCategory getiriliyor. CategoryId: {CategoryId}", menuCategoryUpdateDto.MenuCategoryId);
                var existingMenuCategory = await _menuCategoryRepository.GetById(menuCategoryUpdateDto.MenuCategoryId);

                if (existingMenuCategory == null)
                {
                    _logger.LogWarning("MenuCategory bulunamadı. CategoryId: {CategoryId}", menuCategoryUpdateDto.MenuCategoryId);
                    return new ErrorResult("Güncellenecek kategori bulunamadı.");
                }

                // Validate Cafe exists
                _logger.LogDebug("Cafe kontrolü yapılıyor. CafeId: {CafeId}", menuCategoryUpdateDto.CafeId);
                var cafeResult = await _cafeRepository.GetById(menuCategoryUpdateDto.CafeId);
                if (cafeResult == null || cafeResult.Id == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz CafeId. CafeId: {CafeId}", menuCategoryUpdateDto.CafeId);
                    return new ErrorResult("Kafe bilgisi bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor. CategoryId: {CategoryId}", menuCategoryUpdateDto.MenuCategoryId);
                var strategy = await _menuCategoryRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _menuCategoryRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        Guid? oldImageId = existingMenuCategory.MenuCategoryImageId;
                        ImageFile newImageFile = null;

                        // Handle new image if provided
                        if (newImageData != null && newImageData.Length > 0)
                        {
                            _logger.LogDebug("Yeni ImageFile oluşturuluyor.");

                            newImageFile = new ImageFile
                            {
                                ImageByteFile = newImageData,
                                IsActive = true,
                                ImageContentType = ImageContentType.Category,
                                MenuCategoryId = menuCategoryUpdateDto.MenuCategoryId
                            };

                            await _imageFileRepository.AddAsync(newImageFile);
                            var newImageResult = await _imageFileRepository.SaveChangeAsync();

                            if (newImageResult <= 0)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError("Yeni ImageFile oluşturulamadı.");
                                return new ErrorResult("Yeni resim yüklenirken bir hata oluştu.");
                            }

                            _logger.LogInformation("Yeni ImageFile başarıyla oluşturuldu. ImageId: {ImageId}", newImageFile.Id);
                            menuCategoryUpdateDto.ImageFileId = newImageFile.Id;

                            // Delete old image if exists
                            if (oldImageId.HasValue)
                            {
                                _logger.LogDebug("Eski ImageFile siliniyor. ImageId: {ImageId}", oldImageId.Value);
                                var oldImageFile = await _imageFileRepository.GetById(oldImageId.Value);

                                if (oldImageFile != null)
                                {
                                    await _imageFileRepository.DeleteAsync(oldImageFile);
                                    var deleteOldImageResult = await _imageFileRepository.SaveChangeAsync();

                                    if (deleteOldImageResult <= 0)
                                    {
                                        _logger.LogWarning("Eski ImageFile silinemedi, devam ediliyor. ImageId: {ImageId}", oldImageId.Value);
                                    }
                                    else
                                    {
                                        _logger.LogInformation("Eski ImageFile başarıyla silindi. ImageId: {ImageId}", oldImageId.Value);
                                    }
                                }
                            }
                        }

                        // Update MenuCategory
                        _logger.LogDebug("MenuCategory güncelleniyor. CategoryId: {CategoryId}", menuCategoryUpdateDto.MenuCategoryId);
                        menuCategoryUpdateDto.Adapt(existingMenuCategory);

                        if (newImageFile != null)
                        {
                            existingMenuCategory.MenuCategoryImageId = newImageFile.Id;
                        }

                        await _menuCategoryRepository.UpdateAsync(existingMenuCategory);
                        var categoryUpdateResult = await _menuCategoryRepository.SaveChangeAsync();

                        if (categoryUpdateResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("MenuCategory güncellenemedi. CategoryId: {CategoryId}", menuCategoryUpdateDto.MenuCategoryId);
                            return new ErrorResult("Kategori güncellenirken bir hata oluştu.");
                        }

                        _logger.LogInformation("MenuCategory başarıyla güncellendi. CategoryId: {CategoryId}", menuCategoryUpdateDto.MenuCategoryId);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. MenuCategory başarıyla güncellendi. CategoryId: {CategoryId}, CategoryName: {CategoryName}",
                            menuCategoryUpdateDto.MenuCategoryId, menuCategoryUpdateDto.MenuCategoryName);

                        return new SuccessResult("Kategori başarıyla güncellendi.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "MenuCategory güncelleme sırasında hata oluştu. Transaction rollback yapıldı. CategoryId: {CategoryId}",
                                menuCategoryUpdateDto.MenuCategoryId);
                        }
                        else
                        {
                            _logger.LogError(ex, "MenuCategory güncelleme sırasında hata oluştu. CategoryId: {CategoryId}",
                                menuCategoryUpdateDto.MenuCategoryId);
                        }

                        return new ErrorResult($"Bir hata oluştu: {ex.Message}");
                    }
                    finally
                    {
                        transaction?.Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MenuCategory güncelleme işlemi başarısız oldu. CategoryId: {CategoryId}",
                    menuCategoryUpdateDto.MenuCategoryId);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }
    }
}