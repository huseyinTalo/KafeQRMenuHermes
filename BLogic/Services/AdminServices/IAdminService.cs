using KafeQRMenu.BLogic.DTOs.AdminDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.AdminServices
{
    public interface IAdminService
    {
        Task<IDataResult<List<AdminListDTO>>> GetAllAsync();
        Task<IDataResult<AdminDTO>> GetByIdAsync(Guid Id);
        Task<IDataResult<AdminDTO>> GetByIdAsync(Guid Id, bool tracking);
        Task<IDataResult<AdminDTO>> GetByIdentityIdAsync(string Id, bool tracking);
        Task<IResult> UpdateAsync(AdminUpdateDTO adminUpdateDto, byte[] newImageData = null);
        Task<IResult> CreateAsync(AdminCreateDTO adminCreateDto, byte[] imageData = null);
        Task<IResult> DeleteAsync(AdminDTO adminDto);
    }
}
