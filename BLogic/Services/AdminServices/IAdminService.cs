using BLogic.DTOs.AdminDTOs;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.DataAccess.Repositories.AdminRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.AdminServices
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
