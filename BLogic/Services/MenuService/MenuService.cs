using KafeQRMenu.BLogic.DTOs.MenuDTOs;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Enums;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.CafeRepositories;
using KafeQRMenu.DataAccess.Repositories.ImageRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuRepositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KafeQRMenu.BLogic.Services.MenuService
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMenuCategoryRepository _menuCategoryRepository;
        private readonly ICafeRepository _cafeRepository;
        private readonly IImageFileRepository _imageFileRepository;
        private readonly ILogger<MenuService> _logger;

        public MenuService(
            IMenuRepository menuRepository,
            IMenuCategoryRepository menuCategoryRepository,
            ICafeRepository cafeRepository,
            IImageFileRepository imageFileRepository,
            ILogger<MenuService> logger)
        {
            _menuRepository = menuRepository;
            _menuCategoryRepository = menuCategoryRepository;
            _cafeRepository = cafeRepository;
            _imageFileRepository = imageFileRepository;
            _logger = logger;
        }

        public async Task<IDataResult<List<MenuListDTO>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Tüm menüler getiriliyor.");

                var menus = await _menuRepository.GetAllAsync(
                    orderBy: x => x.CreatedTime,
                    orderByDescending: true
                );

                if (menus == null || !menus.Any())
                {
                    return new SuccessDataResult<List<MenuListDTO>>(
                        new List<MenuListDTO>(),
                        "Kayıt bulunamadı."
                    );
                }

                var menuListDtos = menus.Adapt<List<MenuListDTO>>();

                return new SuccessDataResult<List<MenuListDTO>>(
                    menuListDtos,
                    $"{menuListDtos.Count} adet menü listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync metodunda hata oluştu.");
                return new ErrorDataResult<List<MenuListDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IDataResult<MenuDTO>> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Menü getiriliyor. MenuId: {MenuId}", id);

                if (id == Guid.Empty)
                {
                    return new ErrorDataResult<MenuDTO>(null, "Geçersiz Id.");
                }

                var menu = await _menuRepository.GetByIdWithCategoriesAsync(id);

                if (menu == null)
                {
                    return new ErrorDataResult<MenuDTO>(null, "Menü bulunamadı.");
                }

                var menuDto = menu.Adapt<MenuDTO>();
                menuDto.CategoryIds = menu.CategoriesOfMenu?.Select(c => c.Id).ToList();

                // Image varsa byte array olarak ekle
                if (menuDto.ImageFileId.HasValue && menuDto.ImageFileId != Guid.Empty)
                {
                    var imageData = await _imageFileRepository.GetById(menuDto.ImageFileId.Value);
                    if (imageData?.ImageByteFile != null)
                    {
                        menuDto.ImageFileBytes = imageData.ImageByteFile;
                    }
                }

                foreach(var item in menuDto.Categories)
                {
                    if(item.ImageFileId.HasValue && item.ImageFileId != Guid.Empty)
                    {
                        var imageData = await _imageFileRepository.GetById(item.ImageFileId ?? Guid.Empty);
                        item.ImageFileBytes = imageData.ImageByteFile;
                    }
                }

                return new SuccessDataResult<MenuDTO>(menuDto, "Menü detayları getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync metodunda hata oluştu. MenuId: {MenuId}", id);
                return new ErrorDataResult<MenuDTO>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IDataResult<List<MenuDTO>>> GetAllAsyncCafesCatsItems(Guid cafeId)
        {
            try
            {
                _logger.LogInformation("Cafe menüleri getiriliyor. CafeId: {CafeId}", cafeId);

                var menus = await _menuRepository.GetAllWithDetailsAsync(cafeId);

                if (menus == null || !menus.Any())
                {
                    return new SuccessDataResult<List<MenuDTO>>(
                        new List<MenuDTO>(),
                        "Kayıt bulunamadı."
                    );
                }

                var menuDtos = menus.Select(m =>
                {
                    var dto = m.Adapt<MenuDTO>();
                    dto.CategoryIds = m.CategoriesOfMenu?.Select(c => c.Id).ToList();
                    return dto;
                }).ToList();

                // Her menü için image byte array'i ekle
                foreach (var menuDto in menuDtos)
                {
                    if (menuDto.ImageFileId.HasValue && menuDto.ImageFileId != Guid.Empty)
                    {
                        var imageData = await _imageFileRepository.GetById(menuDto.ImageFileId.Value);
                        if (imageData?.ImageByteFile != null)
                        {
                            menuDto.ImageFileBytes = imageData.ImageByteFile;
                        }
                    }
                }

                return new SuccessDataResult<List<MenuDTO>>(
                    menuDtos,
                    $"{menuDtos.Count} adet menü listelendi."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsyncCafesCatsItems metodunda hata oluştu. CafeId: {CafeId}", cafeId);
                return new ErrorDataResult<List<MenuDTO>>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IResult> CreateAsync(MenuCreateDTO menuCreateDto, byte[] imageData = null)
        {
            _logger.LogInformation("Menu oluşturma işlemi başlatıldı. MenuName: {MenuName}", menuCreateDto.MenuName);

            try
            {
                if (menuCreateDto == null)
                {
                    _logger.LogWarning("Menu oluşturulamadı. DTO boş olamaz.");
                    return new ErrorResult("Menü bilgisi boş olamaz.");
                }

                // Validate Cafe exists
                _logger.LogDebug("Cafe kontrolü yapılıyor. CafeId: {CafeId}", menuCreateDto.CafeId);
                var cafeResult = await _cafeRepository.GetById(menuCreateDto.CafeId);
                if (cafeResult == null || cafeResult.Id == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz CafeId. CafeId: {CafeId}", menuCreateDto.CafeId);
                    return new ErrorResult("Kafe bilgisi bulunamadı.");
                }

                // Validate Categories
                if (menuCreateDto.CategoryIds?.Any() == true)
                {
                    _logger.LogDebug("Kategoriler kontrol ediliyor.");
                    var categories = await _menuCategoryRepository.GetAllAsync(
                        c => menuCreateDto.CategoryIds.Contains(c.Id) && c.CafeId == menuCreateDto.CafeId
                    );

                    if (categories.Count() != menuCreateDto.CategoryIds.Count)
                    {
                        _logger.LogWarning("Bazı kategoriler bulunamadı veya bu cafe'ye ait değil.");
                        return new ErrorResult("Bazı kategoriler bulunamadı veya bu cafe'ye ait değil.");
                    }
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor.");
                var strategy = await _menuRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _menuRepository.BeginTransactionAsync().ConfigureAwait(false);
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
                                ImageContentType = ImageContentType.Menu,
                                MenuId = null // Will be set after Menu is created
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
                        }

                        // Create Menu
                        _logger.LogDebug("Menu entity oluşturuluyor.");
                        var menuEntity = menuCreateDto.Adapt<Menu>();
                        menuEntity.Id = Guid.NewGuid();
                        menuEntity.Cafe = cafeResult;
                        menuEntity.ImageFileId = createdImageFile?.Id;
                        menuEntity.CategoriesOfMenu = new HashSet<MenuCategory>();

                        await _menuRepository.AddAsync(menuEntity);
                        var menuResult = await _menuRepository.SaveChangeAsync();

                        if (menuResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("Menu oluşturulamadı.");
                            return new ErrorResult("Menü oluşturulurken bir hata oluştu.");
                        }

                        _logger.LogInformation("Menu başarıyla oluşturuldu. MenuId: {MenuId}", menuEntity.Id);

                        // Update ImageFile with MenuId if image was created
                        if (createdImageFile != null)
                        {
                            _logger.LogDebug("ImageFile güncelleniyor. MenuId ekleniyor.");
                            createdImageFile.MenuId = menuEntity.Id;
                            await _imageFileRepository.UpdateAsync(createdImageFile);
                            var updateImageResult = await _imageFileRepository.SaveChangeAsync();

                            if (updateImageResult <= 0)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError("ImageFile MenuId ile güncellenemedi.");
                                return new ErrorResult("Resim menü ile ilişkilendirilemedi.");
                            }

                            _logger.LogInformation("ImageFile MenuId ile güncellendi.");
                        }

                        // Add Categories (many-to-many)
                        if (menuCreateDto.CategoryIds?.Any() == true)
                        {
                            _logger.LogDebug("Kategoriler menüye ekleniyor.");

                            var categories = await _menuCategoryRepository.GetAllAsync(
                                c => menuCreateDto.CategoryIds.Contains(c.Id)
                            );

                            foreach (var category in categories)
                            {
                                menuEntity.CategoriesOfMenu.Add(category);
                            }

                            var categoriesResult = await _menuRepository.SaveChangeAsync();

                            if (categoriesResult <= 0)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError("Kategoriler menüye eklenemedi.");
                                return new ErrorResult("Kategoriler eklenirken bir hata oluştu.");
                            }

                            _logger.LogInformation("{Count} kategori menüye eklendi.", categories.Count());
                        }

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. Menu başarıyla oluşturuldu. MenuName: {MenuName}",
                            menuCreateDto.MenuName);

                        return new SuccessResult("Menü başarıyla oluşturuldu.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Menu oluşturma sırasında hata oluştu. Transaction rollback yapıldı. MenuName: {MenuName}",
                                menuCreateDto.MenuName);
                        }
                        else
                        {
                            _logger.LogError(ex, "Menu oluşturma sırasında hata oluştu. MenuName: {MenuName}",
                                menuCreateDto.MenuName);
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
                _logger.LogError(ex, "Menu oluşturma işlemi başarısız oldu. MenuName: {MenuName}",
                    menuCreateDto.MenuName);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> UpdateAsync(MenuUpdateDTO menuUpdateDto, byte[] newImageData = null)
        {
            _logger.LogInformation("Menu güncelleme işlemi başlatıldı. MenuId: {MenuId}, MenuName: {MenuName}",
                menuUpdateDto.MenuId, menuUpdateDto.MenuName);

            try
            {
                if (menuUpdateDto == null || menuUpdateDto.MenuId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz menü bilgisi.");
                    return new ErrorResult("Geçersiz menü bilgisi.");
                }

                // Get existing Menu
                _logger.LogDebug("Mevcut Menu getiriliyor. MenuId: {MenuId}", menuUpdateDto.MenuId);
                var existingMenu = await _menuRepository.GetByIdWithCategoriesAsync(menuUpdateDto.MenuId);

                if (existingMenu == null)
                {
                    _logger.LogWarning("Menu bulunamadı. MenuId: {MenuId}", menuUpdateDto.MenuId);
                    return new ErrorResult("Güncellenecek menü bulunamadı.");
                }

                // Validate Cafe exists
                _logger.LogDebug("Cafe kontrolü yapılıyor. CafeId: {CafeId}", menuUpdateDto.CafeId);
                var cafeResult = await _cafeRepository.GetById(menuUpdateDto.CafeId);
                if (cafeResult == null || cafeResult.Id == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz CafeId. CafeId: {CafeId}", menuUpdateDto.CafeId);
                    return new ErrorResult("Kafe bilgisi bulunamadı.");
                }

                // Validate Categories
                if (menuUpdateDto.CategoryIds?.Any() == true)
                {
                    _logger.LogDebug("Kategoriler kontrol ediliyor.");
                    var categories = await _menuCategoryRepository.GetAllAsync(
                        c => menuUpdateDto.CategoryIds.Contains(c.Id) && c.CafeId == menuUpdateDto.CafeId
                    );

                    if (categories.Count() != menuUpdateDto.CategoryIds.Count)
                    {
                        _logger.LogWarning("Bazı kategoriler bulunamadı veya bu cafe'ye ait değil.");
                        return new ErrorResult("Bazı kategoriler bulunamadı veya bu cafe'ye ait değil.");
                    }
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor. MenuId: {MenuId}", menuUpdateDto.MenuId);
                var strategy = await _menuRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _menuRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        Guid? oldImageId = existingMenu.ImageFileId;
                        ImageFile newImageFile = null;

                        // Handle new image if provided
                        if (newImageData != null && newImageData.Length > 0)
                        {
                            _logger.LogDebug("Yeni ImageFile oluşturuluyor.");

                            newImageFile = new ImageFile
                            {
                                ImageByteFile = newImageData,
                                IsActive = true,
                                ImageContentType = ImageContentType.Menu,
                                MenuId = menuUpdateDto.MenuId
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

                        // Update Menu basic properties
                        _logger.LogDebug("Menu güncelleniyor. MenuId: {MenuId}", menuUpdateDto.MenuId);
                        existingMenu.MenuName = menuUpdateDto.MenuName;
                        existingMenu.IsActive = menuUpdateDto.IsActive;
                        existingMenu.CafeId = menuUpdateDto.CafeId;

                        if (newImageFile != null)
                        {
                            existingMenu.ImageFileId = newImageFile.Id;
                        }

                        // Update Categories (many-to-many)
                        if (menuUpdateDto.CategoryIds != null)
                        {
                            _logger.LogDebug("Kategoriler güncelleniyor.");

                            // Clear existing categories
                            existingMenu.CategoriesOfMenu.Clear();

                            // Add new categories
                            if (menuUpdateDto.CategoryIds.Any())
                            {
                                var categories = await _menuCategoryRepository.GetAllAsync(
                                    c => menuUpdateDto.CategoryIds.Contains(c.Id)
                                );

                                foreach (var category in categories)
                                {
                                    existingMenu.CategoriesOfMenu.Add(category);
                                }

                                _logger.LogInformation("{Count} kategori menüye eklendi.", categories.Count());
                            }
                        }

                        await _menuRepository.UpdateAsync(existingMenu);
                        var menuUpdateResult = await _menuRepository.SaveChangeAsync();

                        if (menuUpdateResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("Menu güncellenemedi. MenuId: {MenuId}", menuUpdateDto.MenuId);
                            return new ErrorResult("Menü güncellenirken bir hata oluştu.");
                        }

                        _logger.LogInformation("Menu başarıyla güncellendi. MenuId: {MenuId}", menuUpdateDto.MenuId);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. Menu başarıyla güncellendi. MenuId: {MenuId}, MenuName: {MenuName}",
                            menuUpdateDto.MenuId, menuUpdateDto.MenuName);

                        return new SuccessResult("Menü başarıyla güncellendi.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Menu güncelleme sırasında hata oluştu. Transaction rollback yapıldı. MenuId: {MenuId}",
                                menuUpdateDto.MenuId);
                        }
                        else
                        {
                            _logger.LogError(ex, "Menu güncelleme sırasında hata oluştu. MenuId: {MenuId}",
                                menuUpdateDto.MenuId);
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
                _logger.LogError(ex, "Menu güncelleme işlemi başarısız oldu. MenuId: {MenuId}",
                    menuUpdateDto.MenuId);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteAsync(MenuDTO menuDto)
        {
            _logger.LogInformation("Menu silme işlemi başlatıldı. MenuId: {MenuId}", menuDto.MenuId);

            try
            {
                if (menuDto == null || menuDto.MenuId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz Menu bilgisi.");
                    return new ErrorResult("Geçersiz menü bilgisi.");
                }

                // Get Menu entity
                _logger.LogDebug("Menu entity getiriliyor. MenuId: {MenuId}", menuDto.MenuId);
                var menuEntity = await _menuRepository.GetById(menuDto.MenuId);

                if (menuEntity == null)
                {
                    _logger.LogWarning("Menu bulunamadı. MenuId: {MenuId}", menuDto.MenuId);
                    return new ErrorResult("Menü bulunamadı.");
                }

                // Begin transaction with execution strategy
                _logger.LogDebug("Transaction başlatılıyor. MenuId: {MenuId}", menuDto.MenuId);
                var strategy = await _menuRepository.CreateExecutionStrategy();

                return await strategy.ExecuteAsync<IResult>(async () =>
                {
                    var transaction = await _menuRepository.BeginTransactionAsync().ConfigureAwait(false);
                    try
                    {
                        ImageFile imageFileToDelete = null;

                        // Get and delete associated ImageFile if exists
                        if (menuEntity.ImageFileId.HasValue)
                        {
                            _logger.LogDebug("İlişkili ImageFile getiriliyor. ImageId: {ImageId}", menuEntity.ImageFileId.Value);
                            imageFileToDelete = await _imageFileRepository.GetById(menuEntity.ImageFileId.Value);

                            if (imageFileToDelete != null)
                            {
                                _logger.LogDebug("ImageFile siliniyor. ImageId: {ImageId}", imageFileToDelete.Id);
                                await _imageFileRepository.DeleteAsync(imageFileToDelete);
                                var imageDeleteResult = await _imageFileRepository.SaveChangeAsync();

                                if (imageDeleteResult <= 0)
                                {
                                    await transaction.RollbackAsync();
                                    _logger.LogError("ImageFile silinemedi. ImageId: {ImageId}", imageFileToDelete.Id);
                                    return new ErrorResult("Menü resmi silinirken bir hata oluştu.");
                                }

                                _logger.LogInformation("ImageFile başarıyla silindi. ImageId: {ImageId}", imageFileToDelete.Id);
                            }
                        }

                        // Delete Menu (categories will be automatically removed from junction table)
                        _logger.LogDebug("Menu siliniyor. MenuId: {MenuId}", menuDto.MenuId);
                        await _menuRepository.DeleteAsync(menuEntity);
                        var menuDeleteResult = await _menuRepository.SaveChangeAsync();

                        if (menuDeleteResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("Menu silinemedi. MenuId: {MenuId}", menuDto.MenuId);
                            return new ErrorResult("Menü silinirken bir hata oluştu.");
                        }

                        _logger.LogInformation("Menu başarıyla silindi. MenuId: {MenuId}", menuDto.MenuId);

                        // Commit transaction
                        await transaction.CommitAsync();
                        _logger.LogInformation("Transaction commit edildi. Menu ve ilişkili ImageFile başarıyla silindi. MenuId: {MenuId}",
                            menuDto.MenuId);

                        return new SuccessResult("Menü başarıyla silindi.");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Menu silme sırasında hata oluştu. Transaction rollback yapıldı. MenuId: {MenuId}",
                                menuDto.MenuId);
                        }
                        else
                        {
                            _logger.LogError(ex, "Menu silme sırasında hata oluştu. MenuId: {MenuId}",
                                menuDto.MenuId);
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
                _logger.LogError(ex, "Menu silme işlemi başarısız oldu. MenuId: {MenuId}", menuDto.MenuId);
                return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
            }
        }

        // MenuService.cs içine ekle

        public async Task<IResult> AssignCategoryToMenuAsync(Guid menuId, Guid categoryId)
        {
            _logger.LogInformation("Menüye kategori atama işlemi başlatıldı. MenuId: {MenuId}, CategoryId: {CategoryId}",
                menuId, categoryId);

            try
            {
                if (menuId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz MenuId.");
                    return new ErrorResult("Geçersiz menü bilgisi.");
                }

                if (categoryId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz CategoryId.");
                    return new ErrorResult("Geçersiz kategori bilgisi.");
                }

                // Get Menu with categories
                _logger.LogDebug("Menu getiriliyor. MenuId: {MenuId}", menuId);
                var menu = await _menuRepository.GetByIdWithCategoriesAsync(menuId);

                if (menu == null)
                {
                    _logger.LogWarning("Menu bulunamadı. MenuId: {MenuId}", menuId);
                    return new ErrorResult("Menü bulunamadı.");
                }

                // Get Category
                _logger.LogDebug("Category getiriliyor. CategoryId: {CategoryId}", categoryId);
                var category = await _menuCategoryRepository.GetById(categoryId);

                if (category == null)
                {
                    _logger.LogWarning("Kategori bulunamadı. CategoryId: {CategoryId}", categoryId);
                    return new ErrorResult("Kategori bulunamadı.");
                }

                // Validate: Category must belong to same Cafe
                if (category.CafeId != menu.CafeId)
                {
                    _logger.LogWarning("Kategori farklı bir cafe'ye ait. MenuCafeId: {MenuCafeId}, CategoryCafeId: {CategoryCafeId}",
                        menu.CafeId, category.CafeId);
                    return new ErrorResult("Bu kategori farklı bir cafe'ye aittir.");
                }

                // Check if already assigned
                if (menu.CategoriesOfMenu.Any(c => c.Id == categoryId))
                {
                    _logger.LogWarning("Kategori zaten bu menüde mevcut. CategoryId: {CategoryId}", categoryId);
                    return new ErrorResult("Bu kategori zaten menüde mevcut.");
                }

                // Add category to menu
                _logger.LogDebug("Kategori menüye ekleniyor.");
                menu.CategoriesOfMenu.Add(category);

                await _menuRepository.UpdateAsync(menu);
                var result = await _menuRepository.SaveChangeAsync();

                if (result <= 0)
                {
                    _logger.LogError("Kategori menüye eklenemedi.");
                    return new ErrorResult("Kategori menüye eklenirken bir hata oluştu.");
                }

                _logger.LogInformation("Kategori başarıyla menüye eklendi. MenuId: {MenuId}, CategoryId: {CategoryId}",
                    menuId, categoryId);

                return new SuccessResult("Kategori başarıyla menüye eklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori menüye eklenirken hata oluştu. MenuId: {MenuId}, CategoryId: {CategoryId}",
                    menuId, categoryId);
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> RemoveCategoryFromMenuAsync(Guid menuId, Guid categoryId)
        {
            _logger.LogInformation("Menüden kategori çıkarma işlemi başlatıldı. MenuId: {MenuId}, CategoryId: {CategoryId}",
                menuId, categoryId);

            try
            {
                if (menuId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz MenuId.");
                    return new ErrorResult("Geçersiz menü bilgisi.");
                }

                if (categoryId == Guid.Empty)
                {
                    _logger.LogWarning("Geçersiz CategoryId.");
                    return new ErrorResult("Geçersiz kategori bilgisi.");
                }

                // Get Menu with categories
                _logger.LogDebug("Menu getiriliyor. MenuId: {MenuId}", menuId);
                var menu = await _menuRepository.GetByIdWithCategoriesAsync(menuId);

                if (menu == null)
                {
                    _logger.LogWarning("Menu bulunamadı. MenuId: {MenuId}", menuId);
                    return new ErrorResult("Menü bulunamadı.");
                }

                // Find category in menu
                var categoryToRemove = menu.CategoriesOfMenu.FirstOrDefault(c => c.Id == categoryId);

                if (categoryToRemove == null)
                {
                    _logger.LogWarning("Kategori bu menüde bulunamadı. CategoryId: {CategoryId}", categoryId);
                    return new ErrorResult("Bu kategori menüde bulunamadı.");
                }

                // Remove category from menu (only removes the relationship, not the category itself)
                _logger.LogDebug("Kategori menüden çıkarılıyor.");
                menu.CategoriesOfMenu.Remove(categoryToRemove);

                await _menuRepository.UpdateAsync(menu);
                var result = await _menuRepository.SaveChangeAsync();

                if (result <= 0)
                {
                    _logger.LogError("Kategori menüden çıkarılamadı.");
                    return new ErrorResult("Kategori menüden çıkarılırken bir hata oluştu.");
                }

                _logger.LogInformation("Kategori başarıyla menüden çıkarıldı (kategori silinmedi). MenuId: {MenuId}, CategoryId: {CategoryId}",
                    menuId, categoryId);

                return new SuccessResult("Kategori menüden çıkarıldı (kategori kendisi silinmedi).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori menüden çıkarılırken hata oluştu. MenuId: {MenuId}, CategoryId: {CategoryId}",
                    menuId, categoryId);
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IDataResult<MenuDTO>> GetActiveByCafeIdAsync(Guid cafeId)
        {
            try
            {
                if (cafeId == Guid.Empty)
                {
                    return new ErrorDataResult<MenuDTO>(null, "Geçersiz Kafe Id.");
                }

                var activeMenu = await _menuRepository.GetAsync(
                    x => x.CafeId == cafeId && x.IsActive == true
                );

                if (activeMenu == null)
                {
                    return new ErrorDataResult<MenuDTO>(null, "Aktif menü bulunamadı.");
                }

                var menuDto = activeMenu.Adapt<MenuDTO>();

                // Load image bytes if exists
                if (menuDto.ImageFileId.HasValue && menuDto.ImageFileId.Value != Guid.Empty)
                {
                    var imageFile = await _imageFileRepository.GetById(menuDto.ImageFileId.Value);
                    if (imageFile != null && imageFile.ImageByteFile != null && imageFile.ImageByteFile.Length > 0)
                    {
                        menuDto.ImageFileBytes = imageFile.ImageByteFile;
                    }
                }

                return new SuccessDataResult<MenuDTO>(menuDto, "Aktif menü getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetActiveByCafeIdAsync metodunda hata oluştu. CafeId: {CafeId}", cafeId);
                return new ErrorDataResult<MenuDTO>(null, $"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}