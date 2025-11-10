using KafeQRMenu.BLogic.DTOs.ImageFileDTOs;
using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Utilities.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.Services.ImageServices
{
    public interface IImageFileService
    {
        Task<IDataResult<List<ImageFileListDTO>>> GetAllAsync();
        Task<IDataResult<ImageFileDTO>> GetByIdAsync(Guid id);
        Task<IDataResult<ImageFileDTO>> CreateAsync(ImageFileCreateDTO imageFileCreateDto); // ✅ Changed return type
        Task<IResult> UpdateAsync(ImageFileUpdateDTO imageFileUpdateDto);
        Task<IResult> DeleteAsync(ImageFileDTO imageFileDto);
        Task<IDataResult<List<ImageFileListDTO>>> GetAllAsyncCached();
        Task<IDataResult<ImageFileDTO>> GetByIdAsyncCached(Guid id);
        void ClearAllCache();
    }
}
