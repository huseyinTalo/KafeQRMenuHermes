using KafeQRMenu.BLogic.DTOs.AdminDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.AdminServices
{
    public interface IAdminService
    {
        Task<IDataResult<List<AdminListDTO>>> GetAllAsync();
        Task<IDataResult<AdminDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(AdminUpdateDTO adminUpdateDto);
        Task<IResult> CreateAsync(AdminCreateDTO adminCreateDto);
        Task<IResult> DeleteAsync(AdminDTO adminDto);
    }
}
