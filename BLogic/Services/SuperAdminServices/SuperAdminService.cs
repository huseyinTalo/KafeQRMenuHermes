using BLogic.DTOs.SuperAdminDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.DataAccess.Repositories.SuperAdminRepositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.SuperAdminServices
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly ISuperAdminRepository _superAdminRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public SuperAdminService(ISuperAdminRepository superAdminRepository, UserManager<IdentityUser> userManager)
        {
            _superAdminRepository = superAdminRepository;
            _userManager = userManager;
        }

        public Task<IResult> CreateAsync(SuperAdminCreateDTO superAdminCreateDto)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> DeleteAsync(SuperAdminDTO superAdminDto)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<SuperAdminListDTO>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<SuperAdminDTO>> GetByIdAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateAsync(SuperAdminUpdateDTO superAdminUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
