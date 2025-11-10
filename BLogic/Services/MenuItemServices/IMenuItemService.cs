using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.MenuItemServices
{
    public interface IMenuItemService
    {
        Task<IDataResult<List<MenuItemListDTO>>> GetAllAsync();
        Task<IDataResult<MenuItemDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(MenuItemUpdateDTO menuItemUpdateDto, byte[] newImageData = null);
        Task<IResult> CreateAsync(MenuItemCreateDTO menuItemCreateDto, byte[] imageData = null);
        Task<IResult> DeleteAsync(MenuItemDTO menuItemDto);
        Task<IDataResult<List<MenuItemListDTO>>> GetAllAsyncCafesCatsItems(Guid CafeId);
    }
}