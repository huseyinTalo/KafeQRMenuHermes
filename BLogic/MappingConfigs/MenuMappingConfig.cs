using KafeQRMenu.BLogic.DTOs.ImageFileDTOs;
using KafeQRMenu.BLogic.DTOs.MenuDTOs;
using KafeQRMenu.Data.Entities;
using Mapster;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.MappingConfigs
{
    public class MenuMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Menu, MenuDTO>()
                .Map(dest => dest.MenuId, src => src.Id)
                .Map(dest => dest.Categories, src => src.CategoriesOfMenu).
                TwoWays();
            config.NewConfig<Menu, MenuListDTO>()
                .Map(dest => dest.MenuId, src => src.Id).TwoWays();
            config.NewConfig<Menu, MenuUpdateDTO>()
                .Map(dest => dest.MenuId, src => src.Id).TwoWays();
        }
    }
}
