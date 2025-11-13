using KafeQRMenu.BLogic.DTOs.CafeDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.CafeServices
{
    public interface ICafeService
    {
        Task<IDataResult<List<CafeListDTO>>> GetAllAsync();
        Task<IDataResult<CafeDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(CafeUpdateDTO cafeUpdateDto);
        Task<IResult> CreateAsync(CafeCreateDTO cafeCreateDto);
        Task<IResult> DeleteAsync(CafeDTO cafeDto);
        Task<IDataResult<CafeDTO>> GetByDomainAsync(string DomainName);
    }
}
