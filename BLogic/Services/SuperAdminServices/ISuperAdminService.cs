using KafeQRMenu.BLogic.DTOs.SuperAdminDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.SuperAdminServices
{
    public interface ISuperAdminService
    {
        Task<IDataResult<List<SuperAdminListDTO>>> GetAllAsync();
        Task<IDataResult<SuperAdminDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(SuperAdminUpdateDTO superAdminUpdateDto);
        Task<IResult> CreateAsync(SuperAdminCreateDTO superAdminCreateDto);
        Task<IResult> DeleteAsync(SuperAdminDTO superAdminDto);
    }
}
