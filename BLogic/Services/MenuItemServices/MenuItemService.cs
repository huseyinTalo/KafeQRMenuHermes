using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.MenuItemRepositories;
using Microsoft.Extensions.Logging;
using Mapster;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;

namespace KafeQRMenu.BLogic.Services.MenuItemServices
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ILogger<MenuItemService> _logger;

        public MenuItemService(IMenuItemRepository menuItemRepository, ILogger<MenuItemService> logger)
        {
            _menuItemRepository = menuItemRepository;
            _logger = logger;
        }

        public async Task<IResult> CreateAsync(MenuItemCreateDTO menuItemCreateDto)
        {
            try
            {
                if (menuItemCreateDto == null)
                {
                    return new ErrorResult("Ürün bilgisi boş olamaz.");
                }


                var menuItemEntity = menuItemCreateDto.Adapt<MenuItem>();

                await _menuItemRepository.AddAsync(menuItemEntity);

                var result = await _menuItemRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Yeni Ürün oluşturuldu. Id: {menuItemEntity.Id}");
                    return new SuccessResult("Ürün başarıyla oluşturuldu.");
                }

                return new ErrorResult("Ürün oluşturulurken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync metodunda hata oluştu.");
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteAsync(MenuItemDTO menuItemDto)
        {
            try
            {
                if (menuItemDto == null || menuItemDto.MenuItemId == Guid.Empty)
                {
                    return new ErrorResult("Geçersiz Ürün bilgisi.");
                }
                var menuItemEntity = await _menuItemRepository.GetById(menuItemDto.MenuItemId, tracking: true);

                if (menuItemEntity == null)
                {
                    return new ErrorResult("Ürün bulunamadı.");
                }

                await _menuItemRepository.DeleteAsync(menuItemEntity);

                var result = await _menuItemRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Ürün silindi. Id: {menuItemDto.MenuItemId}");
                    return new SuccessResult("Ürün başarıyla silindi.");
                }

                return new ErrorResult("Ürün silinirken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync metodunda hata oluştu.");
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
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

        public async Task<IDataResult<MenuItemDTO>> GetByIdAsync(Guid Id)
        {
            try
            {
                if (Id == Guid.Empty)
                {
                    return new ErrorDataResult<MenuItemDTO>(null, "Geçersiz Id.");
                }

                var menuItemEntity = await _menuItemRepository.GetById(Id);

                if (menuItemEntity == null)
                {
                    return new ErrorDataResult<MenuItemDTO>(null, "Ürün bulunamadı.");
                }

                var menuItemyDto = menuItemEntity.Adapt<MenuItemDTO>();

                return new SuccessDataResult<MenuItemDTO>(menuItemyDto, "Ürün detayları getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync metodunda hata oluştu. Id: {Id}", Id);
                return new ErrorDataResult<MenuItemDTO>(
                    null,
                    $"Bir hata oluştu: {ex.Message}"
                );
            }
        }

        public async Task<IResult> UpdateAsync(MenuItemUpdateDTO menuItemUpdateDto)
        {
            try
            {

                if (menuItemUpdateDto == null || menuItemUpdateDto.MenuItemId == Guid.Empty)
                {
                    return new ErrorResult("Geçersiz Ürün bilgisi.");
                }

                var existingMenuItem = await _menuItemRepository.GetById(menuItemUpdateDto.MenuItemId, tracking: true);

                if (existingMenuItem == null)
                {
                    return new ErrorResult("Güncellenecek Ürün bulunamadı.");
                }


                menuItemUpdateDto.Adapt(existingMenuItem);

                await _menuItemRepository.UpdateAsync(existingMenuItem);

                var result = await _menuItemRepository.SaveChangeAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Ürün güncellendi. Id: {menuItemUpdateDto.MenuItemId}");
                    return new SuccessResult("Ürün başarıyla güncellendi.");
                }

                return new ErrorResult("Ürün güncellenirken bir hata oluştu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync metodunda hata oluştu. Id: {Id}", menuItemUpdateDto?.MenuItemId);
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}
