using BLogic.DTOs.AdminDTOs;
using BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.DataAccess.Repositories.MenuItemRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.MenuItemServices
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository;

        public MenuItemService(IMenuItemRepository menuItemRepository)
        {
            _menuItemRepository = menuItemRepository;
        }

        public Task<IResult> CreateAsync(MenuItemCreateDTO menuItemCreateDto)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> DeleteAsync(MenuItemDTO menuItemDto)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<MenuItemListDTO>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<AdminDTO>> GetByIdAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateAsync(MenuItemUpdateDTO menuItemUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
