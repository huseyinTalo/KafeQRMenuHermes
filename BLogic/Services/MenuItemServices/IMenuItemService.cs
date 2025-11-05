using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.MenuItemServices
{
    public interface IMenuItemService
    {
        Task<IDataResult<List<MenuItemListDTO>>> GetAllAsync();
        Task<IDataResult<MenuItemDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(MenuItemUpdateDTO menuItemUpdateDto);
        Task<IResult> CreateAsync(MenuItemCreateDTO menuItemCreateDto);
        Task<IResult> DeleteAsync(MenuItemDTO menuItemDto);
    }
}
