using KafeQRMenu.BLogic.DTOs.AdminDTOs;
using KafeQRMenu.Data.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.BLogic.MappingConfigs
{
    public class AdminMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Admin, AdminListDTO>()
                .Map(dest => dest.CafeName, src => src.Cafe.CafeName);
        }
    }
}
