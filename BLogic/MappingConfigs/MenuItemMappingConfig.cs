using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.BLogic.DTOs.MenuItemDTOs;
using KafeQRMenu.Data.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.MappingConfigs
{
    public class MenuItemMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MenuItem, MenuItemDTO>()
             .Map(dest => dest.MenuItemId, src => src.Id)
             .Map(dest => dest.MenuCategoryName, src => src.MenuCategory.MenuCategoryName)
             .TwoWays();

            config.NewConfig<MenuItem, MenuItemListDTO>()
                .Map(dest => dest.MenuItemId, src => src.Id)
                .Map(dest => dest.MenuCategoryName, src => src.MenuCategory.MenuCategoryName)
                .TwoWays();

            config.NewConfig<MenuItem, MenuItemUpdateDTO>()
                .Map(dest => dest.MenuItemId, src => src.Id)
                .Map(dest => dest.MenuCategoryName, src => src.MenuCategory.MenuCategoryName)
                .TwoWays();

            config.NewConfig<MenuItem, MenuItemCreateDTO>()
                .Map(dest => dest.MenuCategoryName, src => src.MenuCategory.MenuCategoryName)
                .TwoWays();
        }
    }
}
