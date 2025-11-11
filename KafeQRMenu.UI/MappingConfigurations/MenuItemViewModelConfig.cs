// KafeQRMenu.UI/MappingConfigs/MenuItemViewModelMappingConfig.cs
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.UI.Areas.Admin.ViewModels.Menu;
using Mapster;

namespace KafeQRMenu.UI.MappingConfigs
{
    public class MenuItemViewModelMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // MenuItemListDTO → ItemListItemViewModel
            config.NewConfig<MenuItemListDTO, ItemListItemViewModel>()
                .Map(dest => dest.ItemId, src => src.MenuItemId)
                .Map(dest => dest.ItemName, src => src.MenuItemName)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.SortOrder, src => src.SortOrder)
                .Map(dest => dest.ImageFileId, src => src.ImageFileId)
                .Map(dest => dest.ImageFileBytes, src => src.ImageFileBytes)
                .Map(dest => dest.CreatedTime, src => src.CreatedTime)
                .Map(dest => dest.CanEdit, src => true)
                .Map(dest => dest.CanDelete, src => true);

            // MenuItemDTO → ItemEditViewModel
            config.NewConfig<MenuItemDTO, ItemEditViewModel>()
                .Map(dest => dest.ItemId, src => src.MenuItemId)
                .Map(dest => dest.ItemName, src => src.MenuItemName)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.OriginalPrice, src => src.Price)
                .Map(dest => dest.SortOrder, src => src.SortOrder)
                .Map(dest => dest.ImageFileId, src => src.ImageFileId)
                .Map(dest => dest.CurrentImageBytes, src => src.ImageFileBytes)
                .Map(dest => dest.CreatedTime, src => src.CreatedTime)
                .Map(dest => dest.UpdatedTime, src => src.UpdatedTime);
        }
    }
}