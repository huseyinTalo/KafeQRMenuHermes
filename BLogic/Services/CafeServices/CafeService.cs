using Microsoft.Extensions.Logging;
using Mapster;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.CafeRepositories;
using KafeQRMenu.BLogic.DTOs.CafeDTOs;
using KafeQRMenu.BLogic.Services.CafeServices;

public class CafeService : ICafeService
{
    private readonly ICafeRepository _cafeRepository;
    private readonly ILogger<CafeService> _logger;

    public CafeService(
        ICafeRepository cafeRepository,
        ILogger<CafeService> logger)
    {
        _cafeRepository = cafeRepository ?? throw new ArgumentNullException(nameof(cafeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IResult> CreateAsync(CafeCreateDTO cafeCreateDto)
    {
        try
        {
            // Validation
            if (cafeCreateDto == null)
            {
                return new ErrorResult("Cafe bilgisi boş olamaz.");
            }

            // DTO'dan Entity'ye Mapster ile mapping
            var cafeEntity = cafeCreateDto.Adapt<Cafe>();

            // Repository'ye ekle
            await _cafeRepository.AddAsync(cafeEntity);

            // Değişiklikleri kaydet
            var result = await _cafeRepository.SaveChangeAsync();

            if (result > 0)
            {
                _logger.LogInformation($"Yeni cafe oluşturuldu. Id: {cafeEntity.Id}");
                return new SuccessResult("Cafe başarıyla oluşturuldu.");
            }

            return new ErrorResult("Cafe oluşturulurken bir hata oluştu.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsync metodunda hata oluştu.");
            return new ErrorResult($"Bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<IResult> DeleteAsync(CafeDTO cafeDto)
    {
        try
        {
            // Validation
            if (cafeDto == null || cafeDto.Id == Guid.Empty)
            {
                return new ErrorResult("Geçersiz cafe bilgisi.");
            }

            // Cafe'yi bul
            var cafeEntity = await _cafeRepository.GetById(cafeDto.Id);

            if (cafeEntity == null)
            {
                return new ErrorResult("Cafe bulunamadı.");
            }

            await _cafeRepository.DeleteAsync(cafeEntity);

            // Değişiklikleri kaydet
            var result = await _cafeRepository.SaveChangeAsync();

            if (result > 0)
            {
                _logger.LogInformation($"Cafe silindi. Id: {cafeDto.Id}");
                return new SuccessResult("Cafe başarıyla silindi.");
            }

            return new ErrorResult("Cafe silinirken bir hata oluştu.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAsync metodunda hata oluştu.");
            return new ErrorResult($"Bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<IDataResult<List<CafeListDTO>>> GetAllAsync()
    {
        try
        {
            // Tüm aktif cafe'leri getir
            var cafes = await _cafeRepository.GetAllAsync(
                orderBy: x => x.CreatedTime,
                orderByDescending: true
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
            // Validation
            if (Id == Guid.Empty)
            {
                return new ErrorDataResult<CafeDTO>(null, "Geçersiz Id.");
            }

            // Cafe'yi bul
            var cafeEntity = await _cafeRepository.GetById(Id);

            if (cafeEntity == null)
            {
                return new ErrorDataResult<CafeDTO>(null, "Cafe bulunamadı.");
            }

            // Entity'den DTO'ya Mapster ile mapping
            var cafeDto = cafeEntity.Adapt<CafeDTO>();

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

    public async Task<IResult> UpdateAsync(CafeUpdateDTO cafeUpdateDto)
    {
        try
        {
            // Validation
            if (cafeUpdateDto == null || cafeUpdateDto.Id == Guid.Empty)
            {
                return new ErrorResult("Geçersiz cafe bilgisi.");
            }

            // Mevcut cafe'yi bul
            var existingCafe = await _cafeRepository.GetById(cafeUpdateDto.Id);

            if (existingCafe == null)
            {
                return new ErrorResult("Güncellenecek cafe bulunamadı.");
            }

            // Mapster ile güncelleme - mevcut entity üzerine map et
            cafeUpdateDto.Adapt(existingCafe);

            // Repository'de güncelle
            await _cafeRepository.UpdateAsync(existingCafe);

            // Değişiklikleri kaydet
            var result = await _cafeRepository.SaveChangeAsync();

            if (result > 0)
            {
                _logger.LogInformation($"Cafe güncellendi. Id: {cafeUpdateDto.Id}");
                return new SuccessResult("Cafe başarıyla güncellendi.");
            }

            return new ErrorResult("Cafe güncellenirken bir hata oluştu.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAsync metodunda hata oluştu. Id: {Id}", cafeUpdateDto?.Id);
            return new ErrorResult($"Bir hata oluştu: {ex.Message}");
        }
    }
}