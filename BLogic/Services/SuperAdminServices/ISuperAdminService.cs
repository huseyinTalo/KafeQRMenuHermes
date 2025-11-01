using BLogic.DTOs.AdminDTOs;
using BLogic.DTOs.SuperAdminDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.SuperAdminServices
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
