using Microsoft.Extensions.Logging;
using Mapster;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Enums;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.CafeRepositories;
using KafeQRMenu.DataAccess.Repositories.ImageRepositories;
using KafeQRMenu.BLogic.DTOs.CafeDTOs;
using KafeQRMenu.BLogic.Services.CafeServices;
using Microsoft.EntityFrameworkCore;

public class CafeService : ICafeService
{
    private readonly ICafeRepository _cafeRepository;
    private readonly IImageFileRepository _imageFileRepository;
    private readonly ILogger<CafeService> _logger;

    public CafeService(
        ICafeRepository cafeRepository,
        IImageFileRepository imageFileRepository,
        ILogger<CafeService> logger)
    {
        _cafeRepository = cafeRepository ?? throw new ArgumentNullException(nameof(cafeRepository));
        _imageFileRepository = imageFileRepository ?? throw new ArgumentNullException(nameof(imageFileRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IResult> CreateAsync(CafeCreateDTO cafeCreateDto)
    {
        return await CreateAsync(cafeCreateDto, null);
    }

    public async Task<IResult> CreateAsync(CafeCreateDTO cafeCreateDto, byte[] imageData)
    {
        _logger.LogInformation("Cafe oluşturma işlemi başlatıldı. CafeName: {CafeName}", cafeCreateDto.CafeName);

        try
        {
            // Validation
            if (cafeCreateDto == null)
            {
                _logger.LogWarning("Cafe oluşturulamadı. DTO boş olamaz.");
                return new ErrorResult("Cafe bilgisi boş olamaz.");
            }

            // Begin transaction with execution strategy
            _logger.LogDebug("Transaction başlatılıyor.");
            var strategy = await _cafeRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IResult>(async () =>
            {
                var transaction = await _cafeRepository.BeginTransactionAsync().ConfigureAwait(false);
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
                            ImageContentType = ImageContentType.Cafe,
                            CafeId = null // Will be set after Cafe is created
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

                    // Create Cafe
                    _logger.LogDebug("Cafe entity oluşturuluyor.");
                    var cafeEntity = cafeCreateDto.Adapt<Cafe>();
                    cafeEntity.Id = Guid.NewGuid();
                    cafeEntity.ImageFileId = createdImageFile?.Id;

                    await _cafeRepository.AddAsync(cafeEntity);
                    var cafeResult = await _cafeRepository.SaveChangeAsync();

                    if (cafeResult <= 0)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Cafe oluşturulamadı.");
                        return new ErrorResult("Cafe oluşturulurken bir hata oluştu.");
                    }

                    _logger.LogInformation("Cafe başarıyla oluşturuldu. CafeId: {CafeId}", cafeEntity.Id);

                    // Update ImageFile with CafeId if image was created
                    if (createdImageFile != null)
                    {
                        _logger.LogDebug("ImageFile güncelleniyor. CafeId ekleniyor.");
                        createdImageFile.CafeId = cafeEntity.Id;
                        await _imageFileRepository.UpdateAsync(createdImageFile);
                        var updateImageResult = await _imageFileRepository.SaveChangeAsync();

                        if (updateImageResult <= 0)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("ImageFile CafeId ile güncellenemedi.");
                            return new ErrorResult("Resim cafe ile ilişkilendirilemedi.");
                        }

                        _logger.LogInformation("ImageFile CafeId ile güncellendi.");
                    }

                    // Commit transaction
                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction commit edildi. Cafe başarıyla oluşturuldu. CafeName: {CafeName}",
                        cafeCreateDto.CafeName);

                    return new SuccessResult("Cafe başarıyla oluşturuldu.");
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Cafe oluşturma sırasında hata oluştu. Transaction rollback yapıldı. CafeName: {CafeName}",
                            cafeCreateDto.CafeName);
                    }
                    else
                    {
                        _logger.LogError(ex, "Cafe oluşturma sırasında hata oluştu. CafeName: {CafeName}",
                            cafeCreateDto.CafeName);
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
            _logger.LogError(ex, "Cafe oluşturma işlemi başarısız oldu. CafeName: {CafeName}",
                cafeCreateDto.CafeName);
            return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<IResult> UpdateAsync(CafeUpdateDTO cafeUpdateDto)
    {
        return await UpdateAsync(cafeUpdateDto, null);
    }

    public async Task<IResult> UpdateAsync(CafeUpdateDTO cafeUpdateDto, byte[] newImageData)
    {
        _logger.LogInformation("Cafe güncelleme işlemi başlatıldı. CafeId: {CafeId}, CafeName: {CafeName}",
            cafeUpdateDto.Id, cafeUpdateDto.CafeName);

        try
        {
            // Validation
            if (cafeUpdateDto == null || cafeUpdateDto.Id == Guid.Empty)
            {
                _logger.LogWarning("Geçersiz cafe bilgisi.");
                return new ErrorResult("Geçersiz cafe bilgisi.");
            }

            // Get existing Cafe
            _logger.LogDebug("Mevcut Cafe getiriliyor. CafeId: {CafeId}", cafeUpdateDto.Id);
            var existingCafe = await _cafeRepository.GetById(cafeUpdateDto.Id);

            if (existingCafe == null)
            {
                _logger.LogWarning("Cafe bulunamadı. CafeId: {CafeId}", cafeUpdateDto.Id);
                return new ErrorResult("Güncellenecek cafe bulunamadı.");
            }

            // Begin transaction with execution strategy
            _logger.LogDebug("Transaction başlatılıyor. CafeId: {CafeId}", cafeUpdateDto.Id);
            var strategy = await _cafeRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IResult>(async () =>
            {
                var transaction = await _cafeRepository.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    Guid? oldImageId = existingCafe.ImageFileId;
                    ImageFile newImageFile = null;

                    // Handle new image if provided
                    if (newImageData != null && newImageData.Length > 0)
                    {
                        _logger.LogDebug("Yeni ImageFile oluşturuluyor.");

                        newImageFile = new ImageFile
                        {
                            ImageByteFile = newImageData,
                            IsActive = true,
                            ImageContentType = ImageContentType.Cafe,
                            CafeId = cafeUpdateDto.Id
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

                    // Update Cafe properties
                    _logger.LogDebug("Cafe güncelleniyor. CafeId: {CafeId}", cafeUpdateDto.Id);

                    // Mapster ile güncelleme - mevcut entity üzerine map et
                    cafeUpdateDto.Adapt(existingCafe);

                    if (newImageFile != null)
                    {
                        existingCafe.ImageFileId = newImageFile.Id;
                    }

                    await _cafeRepository.UpdateAsync(existingCafe);
                    var cafeUpdateResult = await _cafeRepository.SaveChangeAsync();

                    if (cafeUpdateResult <= 0)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Cafe güncellenemedi. CafeId: {CafeId}", cafeUpdateDto.Id);
                        return new ErrorResult("Cafe güncellenirken bir hata oluştu.");
                    }

                    _logger.LogInformation("Cafe başarıyla güncellendi. CafeId: {CafeId}", cafeUpdateDto.Id);

                    // Commit transaction
                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction commit edildi. Cafe başarıyla güncellendi. CafeId: {CafeId}, CafeName: {CafeName}",
                        cafeUpdateDto.Id, cafeUpdateDto.CafeName);

                    return new SuccessResult("Cafe başarıyla güncellendi.");
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Cafe güncelleme sırasında hata oluştu. Transaction rollback yapıldı. CafeId: {CafeId}",
                            cafeUpdateDto.Id);
                    }
                    else
                    {
                        _logger.LogError(ex, "Cafe güncelleme sırasında hata oluştu. CafeId: {CafeId}",
                            cafeUpdateDto.Id);
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
            _logger.LogError(ex, "Cafe güncelleme işlemi başarısız oldu. CafeId: {CafeId}",
                cafeUpdateDto.Id);
            return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<IResult> DeleteAsync(CafeDTO cafeDto)
    {
        _logger.LogInformation("Cafe silme işlemi başlatıldı. CafeId: {CafeId}", cafeDto.Id);

        try
        {
            // Validation
            if (cafeDto == null || cafeDto.Id == Guid.Empty)
            {
                _logger.LogWarning("Geçersiz Cafe bilgisi.");
                return new ErrorResult("Geçersiz cafe bilgisi.");
            }

            // Get Cafe entity
            _logger.LogDebug("Cafe entity getiriliyor. CafeId: {CafeId}", cafeDto.Id);
            var cafeEntity = await _cafeRepository.GetById(cafeDto.Id);

            if (cafeEntity == null)
            {
                _logger.LogWarning("Cafe bulunamadı. CafeId: {CafeId}", cafeDto.Id);
                return new ErrorResult("Cafe bulunamadı.");
            }

            // Begin transaction with execution strategy
            _logger.LogDebug("Transaction başlatılıyor. CafeId: {CafeId}", cafeDto.Id);
            var strategy = await _cafeRepository.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IResult>(async () =>
            {
                var transaction = await _cafeRepository.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    ImageFile imageFileToDelete = null;

                    // Get and delete associated ImageFile if exists
                    if (cafeEntity.ImageFileId.HasValue)
                    {
                        _logger.LogDebug("İlişkili ImageFile getiriliyor. ImageId: {ImageId}", cafeEntity.ImageFileId.Value);
                        imageFileToDelete = await _imageFileRepository.GetById(cafeEntity.ImageFileId.Value);

                        if (imageFileToDelete != null)
                        {
                            _logger.LogDebug("ImageFile siliniyor. ImageId: {ImageId}", imageFileToDelete.Id);
                            await _imageFileRepository.DeleteAsync(imageFileToDelete);
                            var imageDeleteResult = await _imageFileRepository.SaveChangeAsync();

                            if (imageDeleteResult <= 0)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError("ImageFile silinemedi. ImageId: {ImageId}", imageFileToDelete.Id);
                                return new ErrorResult("Cafe resmi silinirken bir hata oluştu.");
                            }

                            _logger.LogInformation("ImageFile başarıyla silindi. ImageId: {ImageId}", imageFileToDelete.Id);
                        }
                    }

                    // Delete Cafe
                    _logger.LogDebug("Cafe siliniyor. CafeId: {CafeId}", cafeDto.Id);
                    await _cafeRepository.DeleteAsync(cafeEntity);
                    var cafeDeleteResult = await _cafeRepository.SaveChangeAsync();

                    if (cafeDeleteResult <= 0)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Cafe silinemedi. CafeId: {CafeId}", cafeDto.Id);
                        return new ErrorResult("Cafe silinirken bir hata oluştu.");
                    }

                    _logger.LogInformation("Cafe başarıyla silindi. CafeId: {CafeId}", cafeDto.Id);

                    // Commit transaction
                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction commit edildi. Cafe ve ilişkili ImageFile başarıyla silindi. CafeId: {CafeId}",
                        cafeDto.Id);

                    return new SuccessResult("Cafe başarıyla silindi.");
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Cafe silme sırasında hata oluştu. Transaction rollback yapıldı. CafeId: {CafeId}",
                            cafeDto.Id);
                    }
                    else
                    {
                        _logger.LogError(ex, "Cafe silme sırasında hata oluştu. CafeId: {CafeId}",
                            cafeDto.Id);
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
            _logger.LogError(ex, "Cafe silme işlemi başarısız oldu. CafeId: {CafeId}", cafeDto.Id);
            return new ErrorResult($"Beklenmeyen bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<IDataResult<List<CafeListDTO>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Tüm cafe'ler getiriliyor.");

            // Tüm aktif cafe'leri getir (tracking = false for performance)
            var cafes = await _cafeRepository.GetAllAsync(
                orderBy: x => x.CreatedTime,
                orderByDescending: true,
                tracking: false
            );

            if (cafes == null || !cafes.Any())
            {
                return new SuccessDataResult<List<CafeListDTO>>(
                    new List<CafeListDTO>(),
                    "Kayıt bulunamadı."
                );
            }

            // Entity'lerden DTO'lara Mapster ile mapping
            var cafeDtoList = cafes.Adapt<List<CafeListDTO>>();

            // Her cafe için image byte array'i ekle
            foreach (var cafeDto in cafeDtoList)
            {
                if (cafeDto.ImageFileId.HasValue && cafeDto.ImageFileId != Guid.Empty)
                {
                    var imageData = await _imageFileRepository.GetById(cafeDto.ImageFileId.Value, tracking: false);
                    if (imageData?.ImageByteFile != null)
                    {
                        cafeDto.ImageFileBytes = imageData.ImageByteFile;
                    }
                }
            }

            return new SuccessDataResult<List<CafeListDTO>>(
                cafeDtoList,
                $"{cafeDtoList.Count} adet cafe listelendi."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync metodunda hata oluştu.");
            return new ErrorDataResult<List<CafeListDTO>>(
                null,
                $"Bir hata oluştu: {ex.Message}"
            );
        }
    }

    public async Task<IDataResult<List<CafeListDTO>>> GetAllAsync(bool tracking)
    {
        try
        {
            _logger.LogInformation("Tüm cafe'ler getiriliyor. Tracking: {Tracking}", tracking);

            // Tüm aktif cafe'leri getir
            var cafes = await _cafeRepository.GetAllAsync(
                orderBy: x => x.CreatedTime,
                orderByDescending: true,
                tracking
            );

            if (cafes == null || !cafes.Any())
            {
                return new SuccessDataResult<List<CafeListDTO>>(
                    new List<CafeListDTO>(),
                    "Kayıt bulunamadı."
                );
            }

            // Entity'lerden DTO'lara Mapster ile mapping
            var cafeDtoList = cafes.Adapt<List<CafeListDTO>>();

            // Her cafe için image byte array'i ekle
            foreach (var cafeDto in cafeDtoList)
            {
                if (cafeDto.ImageFileId.HasValue && cafeDto.ImageFileId != Guid.Empty)
                {
                    var imageData = await _imageFileRepository.GetById(cafeDto.ImageFileId.Value, tracking);
                    if (imageData?.ImageByteFile != null)
                    {
                        cafeDto.ImageFileBytes = imageData.ImageByteFile;
                    }
                }
            }

            return new SuccessDataResult<List<CafeListDTO>>(
                cafeDtoList,
                $"{cafeDtoList.Count} adet cafe listelendi."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync metodunda hata oluştu.");
            return new ErrorDataResult<List<CafeListDTO>>(
                null,
                $"Bir hata oluştu: {ex.Message}"
            );
        }
    }

    public async Task<IDataResult<CafeDTO>> GetByIdAsync(Guid Id)
    {
        try
        {
            _logger.LogInformation("Cafe getiriliyor. CafeId: {CafeId}", Id);

            // Validation
            if (Id == Guid.Empty)
            {
                return new ErrorDataResult<CafeDTO>(null, "Geçersiz Id.");
            }

            // Cafe'yi bul (tracking = false for read-only)
            var cafeEntity = await _cafeRepository.GetById(Id, tracking: false);

            if (cafeEntity == null)
            {
                return new ErrorDataResult<CafeDTO>(null, "Cafe bulunamadı.");
            }

            // Entity'den DTO'ya Mapster ile mapping
            var cafeDto = cafeEntity.Adapt<CafeDTO>();

            // Image varsa byte array olarak ekle
            if (cafeDto.ImageFileId.HasValue && cafeDto.ImageFileId != Guid.Empty)
            {
                var imageData = await _imageFileRepository.GetById(cafeDto.ImageFileId.Value, tracking: false);
                if (imageData?.ImageByteFile != null)
                {
                    cafeDto.ImageFileBytes = imageData.ImageByteFile;
                }
            }

            return new SuccessDataResult<CafeDTO>(cafeDto, "Cafe detayları getirildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByIdAsync metodunda hata oluştu. Id: {Id}", Id);
            return new ErrorDataResult<CafeDTO>(
                null,
                $"Bir hata oluştu: {ex.Message}"
            );
        }
    }

    public async Task<IDataResult<CafeDTO>> GetByIdAsync(Guid Id, bool tracking)
    {
        try
        {
            _logger.LogInformation("Cafe getiriliyor. CafeId: {CafeId}, Tracking: {Tracking}", Id, tracking);

            // Validation
            if (Id == Guid.Empty)
            {
                return new ErrorDataResult<CafeDTO>(null, "Geçersiz Id.");
            }

            // Cafe'yi bul
            var cafeEntity = await _cafeRepository.GetById(Id, tracking);

            if (cafeEntity == null)
            {
                return new ErrorDataResult<CafeDTO>(null, "Cafe bulunamadı.");
            }

            // Entity'den DTO'ya Mapster ile mapping
            var cafeDto = cafeEntity.Adapt<CafeDTO>();

            // Image varsa byte array olarak ekle
            if (cafeDto.ImageFileId.HasValue && cafeDto.ImageFileId != Guid.Empty)
            {
                var imageData = await _imageFileRepository.GetById(cafeDto.ImageFileId.Value, tracking);
                if (imageData?.ImageByteFile != null)
                {
                    cafeDto.ImageFileBytes = imageData.ImageByteFile;
                }
            }

            return new SuccessDataResult<CafeDTO>(cafeDto, "Cafe detayları getirildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByIdAsync metodunda hata oluştu. Id: {Id}", Id);
            return new ErrorDataResult<CafeDTO>(
                null,
                $"Bir hata oluştu: {ex.Message}"
            );
        }
    }

    public async Task<IDataResult<CafeDTO>> GetByDomainAsync(string domainName)
    {
        try
        {
            _logger.LogInformation("Cafe domain ile getiriliyor. Domain: {Domain}", domainName);

            if (string.IsNullOrWhiteSpace(domainName))
            {
                return new ErrorDataResult<CafeDTO>(null, "Geçersiz domain adı.");
            }

            var normalizedDomain = domainName.Trim().ToLowerInvariant();

            // Use GetAsync - returns single entity by expression (tracking = false)
            var cafeEntity = await _cafeRepository.GetAsync(
                c => c.DomainName.ToLower() == normalizedDomain,
                tracking: false
            );

            if (cafeEntity == null)
            {
                return new ErrorDataResult<CafeDTO>(null, "Domain için cafe bulunamadı.");
            }

            var cafeDto = cafeEntity.Adapt<CafeDTO>();

            // Image varsa byte array olarak ekle
            if (cafeDto.ImageFileId.HasValue && cafeDto.ImageFileId != Guid.Empty)
            {
                var imageData = await _imageFileRepository.GetById(cafeDto.ImageFileId.Value, tracking: false);
                if (imageData?.ImageByteFile != null)
                {
                    cafeDto.ImageFileBytes = imageData.ImageByteFile;
                }
            }

            return new SuccessDataResult<CafeDTO>(cafeDto, "Cafe bulundu.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByDomainAsync hata. Domain: {Domain}", domainName);
            return new ErrorDataResult<CafeDTO>(null, $"Hata: {ex.Message}");
        }
    }
}