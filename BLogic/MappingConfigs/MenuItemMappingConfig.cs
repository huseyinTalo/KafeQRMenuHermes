using Mapster;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;

namespace KafeQRMenu.BLogic.MappingConfigs
{
    public class MenuItemMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // MenuItem Entity -> MenuItemDTO
            config.NewConfig<MenuItem, MenuItemDTO>()
                .Map(dest => dest.MenuItemId, src => src.Id)
                .Map(dest => dest.MenuCategoryName, src => src.MenuCategory.MenuCategoryName)
                .Map(dest => dest.ImageFileId, src => src.MenuItemImageId)
                .Map(dest => dest.CreatedTime, src => src.CreatedTime)
                .Map(dest => dest.UpdatedTime, src => src.UpdatedTime)
                .Map(dest => dest.IsActiveOnTheMenu, src => src.IsActiveOnTheMenu)
                .IgnoreNullValues(true);

            // MenuItem Entity -> MenuItemListDTO
            config.NewConfig<MenuItem, MenuItemListDTO>()
                .Map(dest => dest.MenuItemId, src => src.Id)
                .Map(dest => dest.MenuCategoryName, src => src.MenuCategory.MenuCategoryName)
                .Map(dest => dest.ImageFileId, src => src.MenuItemImage.Id)
                .Map(dest => dest.CreatedTime, src => src.CreatedTime)
                .Map(dest => dest.ImageFileBytes, src => src.MenuItemImage.ImageByteFile)
                .Map(dest => dest.IsActiveOnTheMenu, src => src.IsActiveOnTheMenu)
                .TwoWays();

            // MenuItemCreateDTO -> MenuItem Entity
            config.NewConfig<MenuItemCreateDTO, MenuItem>()
                .Map(dest => dest.MenuCategoryId, src => src.MenuCategoryId)
                .Map(dest => dest.MenuItemImageId, src => src.ImageFileId)
                .Map(dest => dest.IsActiveOnTheMenu, src => src.IsActiveOnTheMenu)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.MenuCategory)
                .Ignore(dest => dest.MenuItemImage)
                .Ignore(dest => dest.CreatedTime)
                .Ignore(dest => dest.UpdatedTime)
                .Ignore(dest => dest.DeletedTime);

            // MenuItemUpdateDTO -> MenuItem Entity (for updating existing entity)
            config.NewConfig<MenuItemUpdateDTO, MenuItem>()
                .Map(dest => dest.MenuItemName, src => src.MenuItemName)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.SortOrder, src => src.SortOrder)
                .Map(dest => dest.MenuCategoryId, src => src.MenuCategoryId)
                .Map(dest => dest.MenuItemImageId, src => src.ImageFileId)
                .Map(dest => dest.IsActiveOnTheMenu, src => src.IsActiveOnTheMenu)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.MenuCategory)
                .Ignore(dest => dest.MenuItemImage)
                .Ignore(dest => dest.CreatedTime)
                .Ignore(dest => dest.DeletedTime);
        }
    }
}