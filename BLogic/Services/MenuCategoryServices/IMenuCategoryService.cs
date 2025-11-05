using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.MenuCategoryServices
{
    public interface IMenuCategoryService
    {
        Task<IDataResult<List<MenuCategoryListDTO>>> GetAllAsync();
        Task<IDataResult<MenuCategoryDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(MenuCategoryUpdateDTO menuCategoryUpdateDto);
        Task<IResult> CreateAsync(MenuCategoryCreateDTO menuCategoryCreateDto);
        Task<IResult> DeleteAsync(MenuCategoryDTO menuCategoryDto);
    }
}
