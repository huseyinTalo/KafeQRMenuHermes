using BLogic.DTOs.AdminDTOs;
using BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
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

        public MenuCategoryService(IMenuCategoryRepository menuCategoryRepository)
        {
            _menuCategoryRepository = menuCategoryRepository;
        }

        public Task<IResult> CreateAsync(MenuCategoryCreateDTO menuCategoryCreateDto)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> DeleteAsync(MenuCategoryDTO menuCategoryDto)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<MenuCategoryDTO>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<AdminDTO>> GetByIdAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateAsync(MenuCategoryUpdateDTO menuCategoryUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
