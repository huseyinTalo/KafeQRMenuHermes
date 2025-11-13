using KafeQRMenu.BLogic.DTOs.MenuDTOs;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.Services.MenuService
{
    public interface IMenuService
    {
        Task<IDataResult<List<MenuListDTO>>> GetAllAsync();
        Task<IDataResult<MenuDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(MenuUpdateDTO menuUpdateDto, byte[] newImageData = null);
        Task<IResult> CreateAsync(MenuCreateDTO menuCreateDto, byte[] imageData = null);
        Task<IResult> DeleteAsync(MenuDTO menuItemDto);
        Task<IDataResult<List<MenuDTO>>> GetAllAsyncCafesCatsItems(Guid CafeId);
        Task<IResult> AssignCategoryToMenuAsync(Guid menuId, Guid categoryId);
        Task<IResult> RemoveCategoryFromMenuAsync(Guid menuId, Guid categoryId);
        Task<IDataResult<MenuDTO>> GetActiveByCafeIdAsync(Guid cafeId);
        Task<IDataResult<MenuDTO>> GetActiveByCafeIdAsync(Guid cafeId, bool tracking);
    }
}
