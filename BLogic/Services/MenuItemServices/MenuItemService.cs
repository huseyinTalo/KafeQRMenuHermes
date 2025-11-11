using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.MenuItemRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
using KafeQRMenu.DataAccess.Repositories.ImageRepositories;
using Microsoft.Extensions.Logging;
using Mapster;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace KafeQRMenu.BLogic.Services.MenuItemServices
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<MenuItemService> _logger;
        private readonly IMenuCategoryRepository _menuCategoryRepository;
        private readonly IImageFileRepository _imageFileRepository;

        public MenuItemService(
            IMenuItemRepository menuItemRepository,
            ILogger<MenuItemService> logger,
            IMenuCategoryRepository menuCategoryRepository,
            IImageFileRepository imageFileRepository)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
            _menuCategoryRepository = menuCategoryRepository;
            _imageFileRepository = imageFileRepository;
        }

        public async Task<IResult> CreateAsync(MenuItemCreateDTO menuItemCreateDto, byte[] imageData = null)
        {
            _logger.LogInformation("MenuItem oluşturma işlemi başlatıldı. ItemName: {ItemName}", menuItemCreateDto.MenuItemName);

            try
            {
                if (menuItemCreateDto == null)
                {
                    _logger.LogWarning("MenuItem oluşturulamadı. DTO boş olamaz.");
                    return new ErrorResult("Ürün bilgisi boş olamaz.");
                }

                // Validate MenuCategory exists
                _logger.LogDebug("MenuCategory kontrolü yapılıyor. CategoryId: {CategoryId}", menuItemCreateDto.MenuCategoryId);
                var categoryResult = await _menuCategoryRepository.GetById(menuItemCreateDto.MenuCategoryId);
                if (categoryResult == null || categoryResult.Id == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz MenuCategoryId. CategoryId: {CategoryId}", menuItemCreateDto.MenuCategoryId);
                    return new ErrorResult("Kategori bilgisi bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor.");
                var strategy = await _menuItemRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _menuItemRepository.BeginTransactionAsync().ConfigureAwait(false);
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
                                ImageContentType = ImageContentType.Product,
                                MenuItemId = null // Will be set after MenuItem is created
                            };

                            await _imageFileRepository.AddAsync(createdImageFile);
                            var imageResult = await _imageFileRepository.SaveChangeAsync();

                            if (imageResult <= 0)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError("ImageFile oluşturulamadı.");
                                return new ErrorResult("Resim yüklenirken bir hata oluştu.");
                            }

                            _logger.LogInformation("ImageFile başarıyla oluşturuldu. ImageId: {ImageId}", createdImageFile.Id);
                            menuItemCreateDto.ImageFileId = createdImageFile.Id;
                        }

                        // Create MenuItem
                        _logger.LogDebug("MenuItem entity oluşturuluyor.");
                        var menuItemEntity = menuItemCreateDto.Adapt<MenuItem>();
                        menuItemEntity.MenuCategory = categoryResult;
                        menuItemEntity.MenuItemImageId = menuItemCreateDto.ImageFileId;

                        await _menuItemRepository.AddAsync(menuItemEntity);
                        var itemResult = await _menuItemRepository.SaveChangeAsync();

                        if (itemResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("MenuItem oluşturulamadı.");
                            return new ErrorResult("Ürün oluşturulurken bir hata oluştu.");
                        }

                        _logger.LogInformation("MenuItem başarıyla oluşturuldu. ItemId: {ItemId}", menuItemEntity.Id);

                        // Update ImageFile with MenuItemId if image was created
                        if (createdImageFile != null)
                        {
                            _logger.LogDebug("ImageFile güncelleniyor. MenuItemId ekleniyor.");
                            createdImageFile.MenuItemId = menuItemEntity.Id;
                            await _imageFileRepository.UpdateAsync(createdImageFile);
                            var updateImageResult = await _imageFileRepository.SaveChangeAsync();

                            if (updateImageResult <= 0)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError("ImageFile MenuItemId ile güncellenemedi.");
                                return new ErrorResult("Resim ürün ile ilişkilendirilemedi.");
                            }

                            _logger.LogInformation("ImageFile MenuItemId ile güncellendi.");
                        }

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. MenuItem başarıyla oluşturuldu. ItemName: {ItemName}",
                            menuItemCreateDto.MenuItemName);

                        return new SuccessResult("Ürün başarıyla oluşturuldu.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "MenuItem oluşturma sırasında hata oluştu. Transaction rollback yapıldı. ItemName: {ItemName}",
                                menuItemCreateDto.MenuItemName);
                        }
                        else
                        {
                            _logger.LogError(ex, "MenuItem oluşturma sırasında hata oluştu. ItemName: {ItemName}",
                                menuItemCreateDto.MenuItemName);
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
                _logger.LogError(ex, "MenuItem oluşturma işlemi başarısız oldu. ItemName: {ItemName}",
                    menuItemCreateDto.MenuItemName);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteAsync(MenuItemDTO menuItemDto)
        {
            _logger.LogInformation("MenuItem silme işlemi başlatıldı. ItemId: {ItemId}", menuItemDto.MenuItemId);

            try
            {
                if (menuItemDto == null || menuItemDto.MenuItemId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz MenuItem bilgisi.");
                    return new ErrorResult("Geçersiz Ürün bilgisi.");
                }

                // Get MenuItem entity
                _logger.LogDebug("MenuItem entity getiriliyor. ItemId: {ItemId}", menuItemDto.MenuItemId);
                var menuItemEntity = await _menuItemRepository.GetById(menuItemDto.MenuItemId);

                if (menuItemEntity == null)
                {
                    _logger.LogWarning("MenuItem bulunamadı. ItemId: {ItemId}", menuItemDto.MenuItemId);
                    return new ErrorResult("Ürün bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor. ItemId: {ItemId}", menuItemDto.MenuItemId);
                var strategy = await _menuItemRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _menuItemRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        ImageFile imageFileToDelete = null;

                        // Get and delete associated ImageFile if exists
                        if (menuItemEntity.MenuItemImageId.HasValue)
                        {
                            _logger.LogDebug("İlişkili ImageFile getiriliyor. ImageId: {ImageId}", menuItemEntity.MenuItemImageId.Value);
                            imageFileToDelete = await _imageFileRepository.GetById(menuItemEntity.MenuItemImageId.Value);

                            if (imageFileToDelete != null)
                            {
                                _logger.LogDebug("ImageFile siliniyor. ImageId: {ImageId}", imageFileToDelete.Id);
                                await _imageFileRepository.DeleteAsync(imageFileToDelete);
                                var imageDeleteResult = await _imageFileRepository.SaveChangeAsync();

                                if (imageDeleteResult <= 0)
                                {
                                    await transaction.RollbackAsync();
                                    _logger.LogError("ImageFile silinemedi. ImageId: {ImageId}", imageFileToDelete.Id);
                                    return new ErrorResult("Ürün resmi silinirken bir hata oluştu.");
                                }

                                _logger.LogInformation("ImageFile başarıyla silindi. ImageId: {ImageId}", imageFileToDelete.Id);
                            }
                        }

                        // Delete MenuItem
                        _logger.LogDebug("MenuItem siliniyor. ItemId: {ItemId}", menuItemDto.MenuItemId);
                        await _menuItemRepository.DeleteAsync(menuItemEntity);
                        var itemDeleteResult = await _menuItemRepository.SaveChangeAsync();

                        if (itemDeleteResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("MenuItem silinemedi. ItemId: {ItemId}", menuItemDto.MenuItemId);
                            return new ErrorResult("Ürün silinirken bir hata oluştu.");
                        }

                        _logger.LogInformation("MenuItem başarıyla silindi. ItemId: {ItemId}", menuItemDto.MenuItemId);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. MenuItem ve ilişkili ImageFile başarıyla silindi. ItemId: {ItemId}",
                            menuItemDto.MenuItemId);

                        return new SuccessResult("Ürün başarıyla silindi.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "MenuItem silme sırasında hata oluştu. Transaction rollback yapıldı. ItemId: {ItemId}",
                                menuItemDto.MenuItemId);
                        }
                        else
                        {
                            _logger.LogError(ex, "MenuItem silme sırasında hata oluştu. ItemId: {ItemId}",
                                menuItemDto.MenuItemId);
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
                _logger.LogError(ex, "MenuItem silme işlemi başarısız oldu. ItemId: {ItemId}", menuItemDto.MenuItemId);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IDataResult<List<MenuItemListDTO>>> GetAllAsync()
        {
            try
            {
                var menuItems = await _menuItemRepository.GetAllAsync(
                    orderBy: x => x.CreatedTime,
                    orderByDescending: true
                );

                if (menuItems == null || !menuItems.Any())
                {
                    return new SuccessDataResult<List<MenuItemListDTO>>(
                        new List<MenuItemListDTO>(),
                        "Kayıt bulunamadı."
                    );
                }

                var menuItemDtoList = menuItems.Adapt<List<MenuItemListDTO>>();

                return new SuccessDataResult<List<MenuItemListDTO>>(
                    menuItemDtoList,
                    $"{menuItemDtoList.Count} adet Ürün listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync metodunda hata oluştu.");
                return new ErrorDataResult<List<MenuItemListDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IDataResult<List<MenuItemListDTO>>> GetAllAsyncCafesCatsItems(Guid cafeId)
        {
            try
            {
                // Get category IDs for this cafe
                var menuCategories = await _menuCategoryRepository.GetAllAsync(
                    x => x.CafeId == cafeId,
                    orderBy: x => x.SortOrder,
                    orderByDescending: false
                );

                if (menuCategories == null || !menuCategories.Any())
                {
                    return new SuccessDataResult<List<MenuItemListDTO>>(
                        new List<MenuItemListDTO>(),
                        "Kayıt bulunamadı."
                    );
                }

                var categoryIds = menuCategories.Select(c => c.Id).ToList();

                // Query MenuItems directly from repository to avoid cached navigation properties
                var menuItems = await _menuItemRepository.GetAllAsync(
                    x => categoryIds.Contains(x.MenuCategoryId),
                    orderBy: x => x.SortOrder,
                    orderByDescending: false
                );

                if (menuItems == null || !menuItems.Any())
                {
                    return new SuccessDataResult<List<MenuItemListDTO>>(
                        new List<MenuItemListDTO>(),
                        "Kayıt bulunamadı."
                    );
                }

                var itemList = menuItems.Adapt<List<MenuItemListDTO>>();

                // Load image bytes for each item
                foreach (var item in itemList)
                {
                    if (item.ImageFileId.HasValue && item.ImageFileId.Value != Guid.Empty)
                    {
                        var imageFile = await _imageFileRepository.GetById(item.ImageFileId.Value);
                        if (imageFile != null && imageFile.ImageByteFile != null && imageFile.ImageByteFile.Length > 0)
                        {
                            item.ImageFileBytes = imageFile.ImageByteFile;
                        }
                    }
                }

                return new SuccessDataResult<List<MenuItemListDTO>>(
                    itemList,
                    $"{itemList.Count} adet Ürün listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsyncCafesCatsItems metodunda hata oluştu.");
                return new ErrorDataResult<List<MenuItemListDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IDataResult<MenuItemDTO>> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return new ErrorDataResult<MenuItemDTO>(null, "Geçersiz Id.");
                }

                var menuItemEntity = await _menuItemRepository.GetById(id);

                if (menuItemEntity == null)
                {
                    return new ErrorDataResult<MenuItemDTO>(null, "Ürün bulunamadı.");
                }

                var menuItemDto = menuItemEntity.Adapt<MenuItemDTO>();

                // Load image bytes if exists
                if (menuItemDto.ImageFileId.HasValue && menuItemDto.ImageFileId.Value != Guid.Empty)
                {
                    var imageFile = await _imageFileRepository.GetById(menuItemDto.ImageFileId.Value);
                    if (imageFile != null && imageFile.ImageByteFile != null && imageFile.ImageByteFile.Length > 0)
                    {
                        menuItemDto.ImageFileBytes = imageFile.ImageByteFile;
                    }
                }

                return new SuccessDataResult<MenuItemDTO>(menuItemDto, "Ürün detayları getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync metodunda hata oluştu. Id: {Id}", id);
                return new ErrorDataResult<MenuItemDTO>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IResult> UpdateAsync(MenuItemUpdateDTO menuItemUpdateDto, byte[] newImageData = null)
        {
            _logger.LogInformation("MenuItem güncelleme işlemi başlatıldı. ItemId: {ItemId}, ItemName: {ItemName}",
                menuItemUpdateDto.MenuItemId, menuItemUpdateDto.MenuItemName);

            try
            {
                if (menuItemUpdateDto == null || menuItemUpdateDto.MenuItemId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz Ürün bilgisi.");
                    return new ErrorResult("Geçersiz Ürün bilgisi.");
                }

                // Get existing MenuItem
                _logger.LogDebug("Mevcut MenuItem getiriliyor. ItemId: {ItemId}", menuItemUpdateDto.MenuItemId);
                var existingMenuItem = await _menuItemRepository.GetById(menuItemUpdateDto.MenuItemId);

                if (existingMenuItem == null)
                {
                    _logger.LogWarning("MenuItem bulunamadı. ItemId: {ItemId}", menuItemUpdateDto.MenuItemId);
                    return new ErrorResult("Güncellenecek Ürün bulunamadı.");
                }

                // Validate MenuCategory exists
                _logger.LogDebug("MenuCategory kontrolü yapılıyor. CategoryId: {CategoryId}", menuItemUpdateDto.MenuCategoryId);
                var categoryResult = await _menuCategoryRepository.GetById(menuItemUpdateDto.MenuCategoryId);
                if (categoryResult == null || categoryResult.Id == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz MenuCategoryId. CategoryId: {CategoryId}", menuItemUpdateDto.MenuCategoryId);
                    return new ErrorResult("Kategori bilgisi bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor. ItemId: {ItemId}", menuItemUpdateDto.MenuItemId);
                var strategy = await _menuItemRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _menuItemRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        Guid? oldImageId = existingMenuItem.MenuItemImageId;
                        ImageFile newImageFile = null;

                        // Handle new image if provided
                        if (newImageData != null && newImageData.Length > 0)
                        {
                            // Break relationship FIRST if old image exists (to prevent cascade delete)
                            if (oldImageId.HasValue)
                            {
                                _logger.LogDebug("MenuItem'dan ImageFile ilişkisi kaldırılıyor.");
                                existingMenuItem.MenuItemImageId = null;
                                await _menuItemRepository.UpdateAsync(existingMenuItem);
                                var breakRelationResult = await _menuItemRepository.SaveChangeAsync();

                                if (breakRelationResult <= 0)
                                {
                                    await transaction.RollbackAsync();
                                    _logger.LogError("MenuItem'dan ImageFile ilişkisi kaldırılamadı.");
                                    return new ErrorResult("Resim ilişkisi kaldırılırken bir hata oluştu.");
                                }

                                // Now delete the old image safely
                                _logger.LogDebug("Eski ImageFile siliniyor. ImageId: {ImageId}", oldImageId.Value);
                                var oldImageFile = await _imageFileRepository.GetById(oldImageId.Value);

                                if (oldImageFile != null)
                                {
                                    await _imageFileRepository.DeleteAsync(oldImageFile);
                                    var deleteOldImageResult = await _imageFileRepository.SaveChangeAsync();

                                    if (deleteOldImageResult <= 0)
                                    {
                                        await transaction.RollbackAsync();
                                        _logger.LogError("Eski ImageFile silinemedi. ImageId: {ImageId}", oldImageId.Value);
                                        return new ErrorResult("Eski resim silinirken bir hata oluştu.");
                                    }

                                    _logger.LogInformation("Eski ImageFile başarıyla silindi. ImageId: {ImageId}", oldImageId.Value);
                                }
                            }

                            // Now create new ImageFile
                            _logger.LogDebug("Yeni ImageFile oluşturuluyor.");

                            newImageFile = new ImageFile
                            {
                                ImageByteFile = newImageData,
                                IsActive = true,
                                ImageContentType = ImageContentType.Product,
                                MenuItemId = menuItemUpdateDto.MenuItemId
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
                            menuItemUpdateDto.ImageFileId = newImageFile.Id;
                        }

                        // Update MenuItem
                        _logger.LogDebug("MenuItem güncelleniyor. ItemId: {ItemId}", menuItemUpdateDto.MenuItemId);
                        menuItemUpdateDto.Adapt(existingMenuItem);

                        if (newImageFile != null)
                        {
                            existingMenuItem.MenuItemImageId = newImageFile.Id;
                        }

                        await _menuItemRepository.UpdateAsync(existingMenuItem);
                        var itemUpdateResult = await _menuItemRepository.SaveChangeAsync();

                        if (itemUpdateResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("MenuItem güncellenemedi. ItemId: {ItemId}", menuItemUpdateDto.MenuItemId);
                            return new ErrorResult("Ürün güncellenirken bir hata oluştu.");
                        }

                        _logger.LogInformation("MenuItem başarıyla güncellendi. ItemId: {ItemId}", menuItemUpdateDto.MenuItemId);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. MenuItem başarıyla güncellendi. ItemId: {ItemId}, ItemName: {ItemName}",
                            menuItemUpdateDto.MenuItemId, menuItemUpdateDto.MenuItemName);

                        return new SuccessResult("Ürün başarıyla güncellendi.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "MenuItem güncelleme sırasında hata oluştu. Transaction rollback yapıldı. ItemId: {ItemId}",
                                menuItemUpdateDto.MenuItemId);
                        }
                        else
                        {
                            _logger.LogError(ex, "MenuItem güncelleme sırasında hata oluştu. ItemId: {ItemId}",
                                menuItemUpdateDto.MenuItemId);
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
                _logger.LogError(ex, "MenuItem güncelleme işlemi başarısız oldu. ItemId: {ItemId}",
                    menuItemUpdateDto.MenuItemId);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }
        public async Task<IDataResult<List<MenuItemListDTO>>> GetAllAsyncByCategoryIds(List<Guid> categoryIds)
        {
            try
            {
                if (categoryIds == null || !categoryIds.Any())
                {
                    return new SuccessDataResult<List<MenuItemListDTO>>(
                        new List<MenuItemListDTO>(),
                        "Kategori listesi boş."
                    );
                }

                var menuItems = await _menuItemRepository.GetAllAsync(
                    x => categoryIds.Contains(x.MenuCategoryId),
                    orderBy: x => x.SortOrder,
                    orderByDescending: false
                );

                if (menuItems == null || !menuItems.Any())
                {
                    return new SuccessDataResult<List<MenuItemListDTO>>(
                        new List<MenuItemListDTO>(),
                        "Kayıt bulunamadı."
                    );
                }

                var itemList = menuItems.Adapt<List<MenuItemListDTO>>();

                // Load image bytes for each item
                foreach (var item in itemList)
                {
                    if (item.ImageFileId.HasValue && item.ImageFileId.Value != Guid.Empty)
                    {
                        var imageFile = await _imageFileRepository.GetById(item.ImageFileId.Value);
                        if (imageFile != null && imageFile.ImageByteFile != null && imageFile.ImageByteFile.Length > 0)
                        {
                            item.ImageFileBytes = imageFile.ImageByteFile;
                        }
                    }
                }

                return new SuccessDataResult<List<MenuItemListDTO>>(
                    itemList,
                    $"{itemList.Count} adet ürün listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsyncByCategoryIds metodunda hata oluştu.");
                return new ErrorDataResult<List<MenuItemListDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }
    }
}