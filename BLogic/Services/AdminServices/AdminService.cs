using BLogic.DTOs.AdminDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.DataAccess.Repositories.AdminRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.AdminServices
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public Task<IResult> CreateAsync(AdminCreateDTO adminCreateDto)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> DeleteAsync(AdminDTO adminDto)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<AdminListDTO>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<AdminDTO>> GetByIdAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateAsync(AdminUpdateDTO adminUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
