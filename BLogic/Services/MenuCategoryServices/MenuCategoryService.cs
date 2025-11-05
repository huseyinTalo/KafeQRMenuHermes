using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.CafeRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
using Mapster;
using Microsoft.Extensions.Logging;

namespace KafeQRMenu.BLogic.Services.MenuCategoryServices
{
    public class MenuCategoryService : IMenuCategoryService
    {
        private readonly IMenuCategoryRepository _menuCategoryRepository;
        private readonly ILogger<MenuCategoryService> _logger;
        private readonly ICafeRepository _cafeRepository;

        public MenuCategoryService(IMenuCategoryRepository menuCategoryRepository, ILogger<MenuCategoryService> logger, ICafeRepository cafeRepository)
        {
            _menuCategoryRepository = menuCategoryRepository;
            _logger = logger;
            _cafeRepository = cafeRepository;
        }

        public async Task<IResult> CreateAsync(MenuCategoryCreateDTO menuCategoryCreateDto)
        {
            try
            {
                if (menuCategoryCreateDto == null)
                {
                    return new ErrorResult("Kategori bilgisi boş olamaz.");
                }


                var menuCategoryEntity = menuCategoryCreateDto.Adapt<MenuCategory>();

                var preResult = await _cafeRepository.GetById(menuCategoryCreateDto.CafeId);
                if (preResult.Id == Guid.Empty)
                {
                    return new ErrorResult("Kafe bilgisi boş olamaz");
                }

                menuCategoryEntity.Cafe = preResult;

                await _menuCategoryRepository.AddAsync(menuCategoryEntity);

                var result = await _menuCategoryRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Yeni Kategori oluşturuldu. Id: {menuCategoryEntity.Id}");
                    return new SuccessResult("Kategori başarıyla oluşturuldu.");
                }

                return new ErrorResult("Kategori oluşturulurken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync metodunda hata oluştu.");
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteAsync(MenuCategoryDTO menuCategoryDto)
        {
            try
            {
                if (menuCategoryDto == null || menuCategoryDto.MenuCategoryId == Guid.Empty)
                {
                    return new ErrorResult("Geçersiz Kategori bilgisi.");
                }
                var cafeEntity = await _menuCategoryRepository.GetById(menuCategoryDto.MenuCategoryId);

                if (cafeEntity == null)
                {
                    return new ErrorResult("Kategori bulunamadı.");
                }

                await _menuCategoryRepository.DeleteAsync(cafeEntity);

                var result = await _menuCategoryRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Kategori silindi. Id: {menuCategoryDto.MenuCategoryId}");
                    return new SuccessResult("Kategori başarıyla silindi.");
                }

                return new ErrorResult("Kategori silinirken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync metodunda hata oluştu.");
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
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

        public async Task<IResult> UpdateAsync(MenuCategoryUpdateDTO menuCategoryUpdateDto)
        {
            try
            {

                if (menuCategoryUpdateDto == null || menuCategoryUpdateDto.MenuCategoryId == Guid.Empty)
                {
                    return new ErrorResult("Geçersiz kategori bilgisi.");
                }

                var existingMenuCategory = await _menuCategoryRepository.GetById(menuCategoryUpdateDto.MenuCategoryId);

                if (existingMenuCategory == null)
                {
                    return new ErrorResult("Güncellenecek kategori bulunamadı.");
                }


                menuCategoryUpdateDto.Adapt(existingMenuCategory);

                await _menuCategoryRepository.UpdateAsync(existingMenuCategory);

                var result = await _menuCategoryRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Kategori güncellendi. Id: {menuCategoryUpdateDto.MenuCategoryId}");
                    return new SuccessResult("Kategori başarıyla güncellendi.");
                }

                return new ErrorResult("Kategori güncellenirken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync metodunda hata oluştu. Id: {Id}", menuCategoryUpdateDto?.MenuCategoryId);
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}
