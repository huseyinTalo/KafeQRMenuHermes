using KafeQRMenu.BLogic.DTOs.CafeDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;

namespace KafeQRMenu.BLogic.Services.CafeServices
{
    public interface ICafeService
    {
        Task<IDataResult<List<CafeListDTO>>> GetAllAsync();
        Task<IDataResult<List<CafeListDTO>>> GetAllAsync(bool tracking);
        Task<IDataResult<CafeDTO>> GetByIdAsync(Guid Id);
        Task<IDataResult<CafeDTO>> GetByIdAsync(Guid Id, bool tracking);
        Task<IResult> UpdateAsync(CafeUpdateDTO cafeUpdateDto);
        Task<IResult> UpdateAsync(CafeUpdateDTO cafeUpdateDto, byte[] newImageData);
        Task<IResult> CreateAsync(CafeCreateDTO cafeCreateDto);
        Task<IResult> CreateAsync(CafeCreateDTO cafeCreateDto, byte[] imageData);
        Task<IResult> DeleteAsync(CafeDTO cafeDto);
        Task<IDataResult<CafeDTO>> GetByDomainAsync(string DomainName);
    }
}