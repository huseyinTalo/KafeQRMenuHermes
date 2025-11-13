using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.MenuCategoryServices
{
    public interface IMenuCategoryService
    {
        Task<IDataResult<List<MenuCategoryListDTO>>> GetAllAsync();
        Task<IDataResult<MenuCategoryDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(MenuCategoryUpdateDTO menuCategoryUpdateDto, byte[] newImageData = null);
        Task<IDataResult<MenuCategoryDTO>> CreateAsync(MenuCategoryCreateDTO menuCategoryCreateDto, byte[] imageData = null);
        Task<IResult> DeleteAsync(MenuCategoryDTO menuCategoryDto);
        Task<IDataResult<List<MenuCategoryListDTO>>> GetAllAsyncCafesCats(Guid CafeId);
        Task<IDataResult<List<MenuCategoryListDTO>>> GetAllAsyncByMenuId(Guid menuId);
        Task<IDataResult<List<MenuCategoryListDTO>>> GetAllAsyncByMenuId(Guid menuId, bool tracking);
    }
}