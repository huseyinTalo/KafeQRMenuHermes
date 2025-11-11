using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.DTOs.MenuDTOs;
using KafeQRMenu.UI.Areas.Admin.ViewModels.Menu;
using Mapster;

namespace KafeQRMenu.UI.MappingConfigurations
{
    public class CategoryListItemViewModelConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // MenuCategoryListDTO → CategoryListItemViewModel
            config.NewConfig<MenuCategoryListDTO, CategoryListItemViewModel>()
                .Map(dest => dest.CategoryId, src => src.MenuCategoryId)
                .Map(dest => dest.CategoryName, src => src.MenuCategoryName)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.SortOrder, src => src.SortOrder)
                .Map(dest => dest.ImageFileId, src => src.ImageFileId)
                .Map(dest => dest.ImageFileBytes, src => src.ImageFileBytes)
                .Map(dest => dest.ItemCount, src => src.ItemCount)
                .Map(dest => dest.IsAssignedToMenu, src => false)
                .Map(dest => dest.CanEdit, src => true)
                .Map(dest => dest.CanDelete, src => true)
                .Map(dest => dest.CanViewDetails, src => true);

            config.NewConfig<MenuCategoryDTO, CategoryListItemViewModel>()
                .Map(dest => dest.CategoryId, src => src.MenuCategoryId)
                .Map(dest => dest.CategoryName, src => src.MenuCategoryName)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.SortOrder, src => src.SortOrder)
                .Map(dest => dest.ImageFileId, src => src.ImageFileId)
                .Map(dest => dest.ImageFileBytes, src => src.ImageFileBytes)
                .Map(dest => dest.ItemCount, src => src.ItemCount)
                .Map(dest => dest.IsAssignedToMenu, src => false)
                .Map(dest => dest.CanEdit, src => true)
                .Map(dest => dest.CanDelete, src => true)
                .Map(dest => dest.CanViewDetails, src => true);

            // MenuCategoryDTO → CategoryEditViewModel
            config.NewConfig<MenuCategoryDTO, CategoryEditViewModel>()
                .Map(dest => dest.CategoryId, src => src.MenuCategoryId)
                .Map(dest => dest.CategoryName, src => src.MenuCategoryName)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.SortOrder, src => src.SortOrder)
                .Map(dest => dest.CafeId, src => src.CafeId)
                .Map(dest => dest.ImageFileId, src => src.ImageFileId)
                .Map(dest => dest.CurrentImageBytes, src => src.ImageFileBytes)
                .Map(dest => dest.CreatedTime, src => src.CreatedTime)
                .Map(dest => dest.UpdatedTime, src => src.UpdatedTime);
        }
    }
}
