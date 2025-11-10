using KafeQRMenu.BLogic.DTOs.ImageFileDTOs;
using KafeQRMenu.BLogic.Services.ImageServices;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.Data.Utilities.Abstracts;
using KafeQRMenu.Data.Utilities.Concretes;
using KafeQRMenu.DataAccess.Repositories.ImageRepositories;
using Mapster;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.Services.ImageServices
{
    public class ImageFileService : IImageFileService
    {
        private readonly IImageFileRepository _imageFileRepository;
        private readonly IMemoryCache _memoryCache;

        private const string ALL_IMAGES_CACHE_KEY = "AllImageFiles";
        private const string IMAGE_CACHE_KEY_PREFIX = "ImageFile_";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public ImageFileService(
            IImageFileRepository imageFileRepository,
            IMemoryCache memoryCache)
        {
            _imageFileRepository = imageFileRepository;
            _memoryCache = memoryCache;
        }

        public async Task<IDataResult<List<ImageFileListDTO>>> GetAllAsync()
        {
            try
            {
                var imageFiles = await _imageFileRepository.GetAllAsync(tracking: false);
                var imageFileDTOs = imageFiles.Adapt<List<ImageFileListDTO>>();

                return new SuccessDataResult<List<ImageFileListDTO>>(
                    imageFileDTOs,
                    "Image files retrieved successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<ImageFileListDTO>>(
                    null,
                    $"An error occurred while retrieving image files: {ex.Message}");
            }
        }

        public async Task<IDataResult<ImageFileDTO>> GetByIdAsync(Guid id)
        {
            try
            {
                var imageFile = await _imageFileRepository.GetById(id, tracking: false);

                if (imageFile == null)
                {
                    return new ErrorDataResult<ImageFileDTO>(
                        null,
                        $"Image file with ID {id} not found.");
                }

                var imageFileDTO = imageFile.Adapt<ImageFileDTO>();

                return new SuccessDataResult<ImageFileDTO>(
                    imageFileDTO,
                    "Image file retrieved successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<ImageFileDTO>(
                    null,
                    $"An error occurred while retrieving image file: {ex.Message}");
            }
        }

        // ✅ Updated to return Guid
        public async Task<IDataResult<ImageFileDTO>> CreateAsync(ImageFileCreateDTO imageFileCreateDto)
        {
            try
            {
                if (imageFileCreateDto == null)
                {
                    return new ErrorDataResult<ImageFileDTO>(new ImageFileDTO(), "Image file data cannot be null.");
                }

                if (imageFileCreateDto.ImageByteFile == null || imageFileCreateDto.ImageByteFile.Length == 0)
                {
                    return new ErrorDataResult<ImageFileDTO>(new ImageFileDTO(), "Image file content cannot be empty.");
                }

                // Validate that at least one foreign key is set based on content type
                if (!ValidateForeignKeys(imageFileCreateDto))
                {
                    return new ErrorDataResult<ImageFileDTO>(new ImageFileDTO(), "Invalid foreign key configuration for the specified image content type.");
                }

                var imageFile = imageFileCreateDto.Adapt<ImageFile>();
                imageFile.IsActive = true;

                await _imageFileRepository.AddAsync(imageFile);
                await _imageFileRepository.SaveChangeAsync();

                // Clear cache after creation
                ClearAllCache();

                return new SuccessDataResult<ImageFileDTO>(imageFile.Adapt<ImageFileDTO>(), "Image file created successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<ImageFileDTO>(new ImageFileDTO(), $"An error occurred while creating image file: {ex.Message}");
            }
        }

        public async Task<IResult> UpdateAsync(ImageFileUpdateDTO imageFileUpdateDto)
        {
            try
            {
                if (imageFileUpdateDto == null)
                {
                    return new ErrorResult("Image file data cannot be null.");
                }

                var existingImageFile = await _imageFileRepository.GetById(imageFileUpdateDto.ImageId, tracking: true);

                if (existingImageFile == null)
                {
                    return new ErrorResult($"Image file with ID {imageFileUpdateDto.ImageId} not found.");
                }

                if (imageFileUpdateDto.ImageByteFile == null || imageFileUpdateDto.ImageByteFile.Length == 0)
                {
                    return new ErrorResult("Image file content cannot be empty.");
                }

                // Validate foreign keys
                if (!ValidateForeignKeys(imageFileUpdateDto))
                {
                    return new ErrorResult("Invalid foreign key configuration for the specified image content type.");
                }

                imageFileUpdateDto.Adapt(existingImageFile);
                await _imageFileRepository.UpdateAsync(existingImageFile);
                await _imageFileRepository.SaveChangeAsync();

                // Clear cache after update
                ClearAllCache();

                return new SuccessResult("Image file updated successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"An error occurred while updating image file: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteAsync(ImageFileDTO imageFileDto)
        {
            try
            {
                if (imageFileDto == null)
                {
                    return new ErrorResult("Image file data cannot be null.");
                }

                var imageFile = await _imageFileRepository.GetById(imageFileDto.ImageId, tracking: true);

                if (imageFile == null)
                {
                    return new ErrorResult($"Image file with ID {imageFileDto.ImageId} not found.");
                }

                await _imageFileRepository.DeleteAsync(imageFile);
                await _imageFileRepository.SaveChangeAsync();

                // Clear cache after deletion
                ClearAllCache();

                return new SuccessResult("Image file deleted successfully.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"An error occurred while deleting image file: {ex.Message}");
            }
        }

        // Cached operations
        public async Task<IDataResult<List<ImageFileListDTO>>> GetAllAsyncCached()
        {
            try
            {
                if (_memoryCache.TryGetValue(ALL_IMAGES_CACHE_KEY, out List<ImageFileListDTO> cachedImages))
                {
                    return new SuccessDataResult<List<ImageFileListDTO>>(
                        cachedImages,
                        "Image files retrieved from cache.");
                }

                var result = await GetAllAsync();

                if (result.IsSuccess && result.Data != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(_cacheExpiration)
                        .SetAbsoluteExpiration(TimeSpan.FromHours(2))
                        .SetPriority(CacheItemPriority.Normal);

                    _memoryCache.Set(ALL_IMAGES_CACHE_KEY, result.Data, cacheOptions);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<ImageFileListDTO>>(
                    null,
                    $"An error occurred while retrieving cached image files: {ex.Message}");
            }
        }

        public async Task<IDataResult<ImageFileDTO>> GetByIdAsyncCached(Guid id)
        {
            try
            {
                string cacheKey = $"{IMAGE_CACHE_KEY_PREFIX}{id}";

                if (_memoryCache.TryGetValue(cacheKey, out ImageFileDTO cachedImage))
                {
                    return new SuccessDataResult<ImageFileDTO>(
                        cachedImage,
                        "Image file retrieved from cache.");
                }

                var result = await GetByIdAsync(id);

                if (result.IsSuccess && result.Data != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(_cacheExpiration)
                        .SetAbsoluteExpiration(TimeSpan.FromHours(2))
                        .SetPriority(CacheItemPriority.Normal);

                    _memoryCache.Set(cacheKey, result.Data, cacheOptions);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<ImageFileDTO>(
                    null,
                    $"An error occurred while retrieving cached image file: {ex.Message}");
            }
        }

        public void ClearAllCache()
        {
            _memoryCache.Remove(ALL_IMAGES_CACHE_KEY);
        }

        // Helper methods
        private bool ValidateForeignKeys(ImageFileCreateDTO dto)
        {
            return dto.ImageContentType switch
            {
                Data.Enums.ImageContentType.Category => dto.MenuCategoryId.HasValue,
                Data.Enums.ImageContentType.Product => dto.MenuItemId.HasValue,
                Data.Enums.ImageContentType.Cafe => dto.CafeId.HasValue,
                Data.Enums.ImageContentType.Person => dto.AdminId.HasValue || dto.SuperAdminId.HasValue,
                Data.Enums.ImageContentType.Background => dto.CafeId.HasValue,
                Data.Enums.ImageContentType.Misc => true,
                _ => false
            };
        }

        private bool ValidateForeignKeys(ImageFileUpdateDTO dto)
        {
            return dto.ImageContentType switch
            {
                Data.Enums.ImageContentType.Category => dto.MenuCategoryId.HasValue,
                Data.Enums.ImageContentType.Product => dto.MenuItemId.HasValue,
                Data.Enums.ImageContentType.Cafe => dto.CafeId.HasValue,
                Data.Enums.ImageContentType.Person => dto.AdminId.HasValue || dto.SuperAdminId.HasValue,
                Data.Enums.ImageContentType.Background => dto.CafeId.HasValue,
                Data.Enums.ImageContentType.Misc => true,
                _ => false
            };
        }
    }
}