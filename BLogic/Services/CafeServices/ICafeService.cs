using BLogic.DTOs.AdminDTOs;
using BLogic.DTOs.CafeDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.CafeServices
{
    public interface ICafeService
    {
        Task<IDataResult<List<CafeListDTO>>> GetAllAsync();
        Task<IDataResult<CafeDTO>> GetByIdAsync(Guid Id);
        Task<IResult> UpdateAsync(CafeUpdateDTO cafeUpdateDto);
        Task<IResult> CreateAsync(CafeCreateDTO cafeCreateDto);
        Task<IResult> DeleteAsync(CafeDTO cafeDto);
    }
}
