using KafeQRMenu.BLogic.DTOs.AdminDTOs;
using KafeQRMenu.BLogic.DTOs.MenuCategoryDTOs;
using KafeQRMenu.Data.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.MappingConfigs
{
    public class MenuCategoryMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // MenuCategory → MenuCategoryDTO
            config.NewConfig<MenuCategory, MenuCategoryDTO>()
                .Map(dest => dest.MenuCategoryId, src => src.Id)
                .Map(dest => dest.CafeName, src => src.Cafe.CafeName)
                .Map(dest => dest.ImageFileId, src => src.MenuCategoryImageId)
                .Map(dest => dest.ItemCount, src => src.MenuItems.Count)
                .TwoWays();

            // MenuCategory → MenuCategoryListDTO
            config.NewConfig<MenuCategory, MenuCategoryListDTO>()
                .Map(dest => dest.MenuCategoryId, src => src.Id)
                .Map(dest => dest.CafeName, src => src.Cafe.CafeName)
                .Map(dest => dest.ImageFileId, src => src.MenuCategoryImageId)
                .Map(dest => dest.ItemCount, src => src.MenuItems.Count) // ✅ ImageFileId mapping
                .TwoWays();

            // MenuCategory → MenuCategoryUpdateDTO
            config.NewConfig<MenuCategory, MenuCategoryUpdateDTO>()
                .Map(dest => dest.MenuCategoryId, src => src.Id)
                .Map(dest => dest.CafeName, src => src.Cafe.CafeName)
                .Map(dest => dest.ImageFileId, src => src.MenuCategoryImageId)  // ✅ ImageFileId mapping
                .TwoWays();

            // MenuCategory → MenuCategoryCreateDTO
            config.NewConfig<MenuCategory, MenuCategoryCreateDTO>()
                .Map(dest => dest.CafeName, src => src.Cafe.CafeName)
                .Map(dest => dest.ImageFileId, src => src.MenuCategoryImageId)  // ✅ ImageFileId mapping
                .TwoWays();
        }
    }
}