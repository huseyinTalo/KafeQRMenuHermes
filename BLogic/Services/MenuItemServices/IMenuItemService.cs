using BLogic.DTOs.AdminDTOs;
using BLogic.DTOs.MenuCategoryDTOs;
using BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.MenuItemServices
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
