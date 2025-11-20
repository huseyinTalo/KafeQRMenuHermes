// KafeQRMenu.UI/MappingConfigs/MenuViewModelMappingConfig.cs
using KafeQRMenu.BLogic.DTOs.MenuDTOs;
using KafeQRMenu.Data.Entities;
using KafeQRMenu.UI.Areas.Admin.ViewModels.Menu;
using Mapster;

namespace KafeQRMenu.UI.MappingConfigs
{
    public class MenuViewModelMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // MenuListDTO → MenuListItemViewModel
            config.NewConfig<MenuListDTO, MenuListItemViewModel>()
                .IgnoreNonMapped(true)
                .Map(dest => dest.MenuId, src => src.MenuId)
                .Map(dest => dest.MenuName, src => src.MenuName)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.ImageFileId, src => src.ImageFileId)
                .Map(dest => dest.ImageFileBytes, src => src.ImageFileBytes)
                .Map(dest => dest.CafeId, src => src.CafeId)
                .Map(dest => dest.CafeName, src => src.CafeName)
                .Map(dest => dest.CategoryCount, src => 0)
                .Map(dest => dest.CreatedTime, src => DateTime.Now)
                .Map(dest => dest.CanEdit, src => true)
                .Map(dest => dest.CanDelete, src => true)
                .Map(dest => dest.CanViewDetails, src => true)
                .Map(dest => dest.CanToggleActive, src => true);

            // MenuDTO → MenuEditViewModel
            config.NewConfig<MenuDTO, MenuEditViewModel>()
                .IgnoreNonMapped(true)
                .Map(dest => dest.MenuId, src => src.MenuId)
                .Map(dest => dest.MenuName, src => src.MenuName)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.CafeId, src => src.CafeId)
                .Map(dest => dest.ImageFileId, src => src.ImageFileId)
                .Map(dest => dest.CategoryIds, src => src.CategoryIds)
                .Map(dest => dest.CurrentImageBytes, src => src.ImageFileBytes)
                .Map(dest => dest.DisplayDate, src => src.DisplayDate).TwoWays();

            // MenuDTO → MenuListItemViewModel entity
            config.NewConfig<MenuDTO, MenuListItemViewModel>()
    .IgnoreNonMapped(true)
    .Map(dest => dest.MenuId, src => src.MenuId)
    .Map(dest => dest.MenuName, src => src.MenuName)
    .Map(dest => dest.IsActive, src => src.IsActive)
    .Map(dest => dest.ImageFileId, src => src.ImageFileId)
    .Map(dest => dest.ImageFileBytes, src => src.ImageFileBytes)
    .Map(dest => dest.CafeId, src => src.CafeId)
    .Map(dest => dest.CafeName, src => src.CafeName)
    .Map(dest => dest.CategoryCount, src => src.CategoryIds != null ? src.CategoryIds.Count : 0)
    .Map(dest => dest.CreatedTime, src => src.CreatedTime)
    .Map(dest => dest.CanEdit, src => true)
    .Map(dest => dest.CanDelete, src => true)
    .Map(dest => dest.CanViewDetails, src => true)
    .Map(dest => dest.CanToggleActive, src => true);
        }
    }
}