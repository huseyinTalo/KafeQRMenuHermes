using BLogic.DTOs.AdminDTOs;
using BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.MenuCategoryServices
{
    public interface IMenuCategoryService
    {
        Task<IDataResult<List<MenuCategoryDTO>>> GetAllAsync();
        Task<IDataResult<AdminDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(MenuCategoryUpdateDTO menuCategoryUpdateDto);
        Task<IResult> CreateAsync(MenuCategoryCreateDTO menuCategoryCreateDto);
        Task<IResult> DeleteAsync(MenuCategoryDTO menuCategoryDto);
    }
}
