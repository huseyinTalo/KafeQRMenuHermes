using BLogic.DTOs.CafeDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.DataAccess.Repositories.CafeRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Services.CafeServices
{
    public class CafeService : ICafeService
    {
        private readonly ICafeRepository _cafeRepository;

        public CafeService(ICafeRepository cafeRepository)
        {
            _cafeRepository = cafeRepository;
        }

        public Task<IResult> CreateAsync(CafeCreateDTO cafeCreateDto)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> DeleteAsync(CafeDTO cafeDto)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<CafeListDTO>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<CafeDTO>> GetByIdAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateAsync(CafeUpdateDTO cafeUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
