using BLogic.DTOs.AdminDTOs;
using BLogic.DTOs.CafeDTOs;
using BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.MenuCategoryServices
{
    public class MenuCategoryService : IMenuCategoryService
    {
        private readonly IMenuCategoryRepository _menuCategoryRepository;
        private readonly ILogger<MenuCategoryService> _logger;

        public MenuCategoryService(IMenuCategoryRepository menuCategoryRepository, ILogger<MenuCategoryService> logger)
        {
            _menuCategoryRepository = menuCategoryRepository;
            _logger = logger;
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

                await _menuCategoryRepository.AddAsync(menuCategoryEntity);

                var result = await _menuCategoryRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Yeni cafe oluşturuldu. Id: {menuCategoryEntity.Id}");
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

        public async Task<IResult> DeleteAsync(MenuCategoryDTO menuCategoryDto)
        {
            try
            {
                if (menuCategoryDto == null || menuCategoryDto.MenuCategoryId == Guid.Empty)
                {
                    return new ErrorResult("Geçersiz cafe bilgisi.");
                }
                var cafeEntity = await _menuCategoryRepository.GetById(menuCategoryDto.MenuCategoryId, tracking: true);

                if (cafeEntity == null)
                {
                    return new ErrorResult("Cafe bulunamadı.");
                }

                await _menuCategoryRepository.DeleteAsync(cafeEntity);

                var result = await _menuCategoryRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Cafe silindi. Id: {menuCategoryDto.MenuCategoryId}");
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

        public async Task<IDataResult<List<MenuCategoryListDTO>>> GetAllAsync()
        {
            try
            {
                var menuCategories = await _menuCategoryRepository.GetAllAsync(
                    orderBy: x => x.CreatedTime,
                    orderByDescending: true,
                    tracking: false
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
                    $"{menuCategoryDtoList.Count} adet cafe listelendi."
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

                var menuCategoryEntity = await _menuCategoryRepository.GetById(Id, tracking: false);

                if (menuCategoryEntity == null)
                {
                    return new ErrorDataResult<MenuCategoryDTO>(null, "Cafe bulunamadı.");
                }

                var menuCategoryDto = menuCategoryEntity.Adapt<MenuCategoryDTO>();

                return new SuccessDataResult<MenuCategoryDTO>(menuCategoryDto, "Cafe detayları getirildi.");
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

                var existingMenuCategory = await _menuCategoryRepository.GetById(menuCategoryUpdateDto.MenuCategoryId, tracking: true);

                if (existingMenuCategory == null)
                {
                    return new ErrorResult("Güncellenecek kategori bulunamadı.");
                }

                
                menuCategoryUpdateDto.Adapt(existingMenuCategory);

                await _menuCategoryRepository.UpdateAsync(existingMenuCategory);

                var result = await _menuCategoryRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Cafe güncellendi. Id: {menuCategoryUpdateDto.MenuCategoryId}");
                    return new SuccessResult("Cafe başarıyla güncellendi.");
                }

                return new ErrorResult("Cafe güncellenirken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync metodunda hata oluştu. Id: {Id}", menuCategoryUpdateDto?.MenuCategoryId);
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}
