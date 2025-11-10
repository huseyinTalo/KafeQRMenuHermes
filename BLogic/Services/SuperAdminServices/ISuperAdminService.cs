using KafeQRMenu.BLogic.DTOs.SuperAdminDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.SuperAdminServices
{
    public interface ISuperAdminService
    {
        Task<IDataResult<List<SuperAdminListDTO>>> GetAllAsync();
        Task<IDataResult<SuperAdminDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(SuperAdminUpdateDTO superAdminUpdateDto, byte[] newImageData = null);
        Task<IResult> CreateAsync(SuperAdminCreateDTO superAdminCreateDto, byte[] imageData = null);
        Task<IResult> DeleteAsync(SuperAdminDTO superAdminDto);
    }
}