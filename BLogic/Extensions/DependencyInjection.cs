using BLogic.Services.AdminServices;
using BLogic.Services.CafeServices;
using BLogic.Services.MenuCategoryServices;
using BLogic.Services.MenuItemServices;
using BLogic.Services.SuperAdminServices;
using KafeQRMenu.DataAccess.AppContext;
using KafeQRMenu.DataAccess.Repositories.AdminRepositories;
using KafeQRMenu.DataAccess.Repositories.CafeRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuItemRepositories;
using KafeQRMenu.DataAccess.Repositories.SuperAdminRepositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLogic.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBLogicServices(this IServiceCollection services)
        {
         
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ISuperAdminService, SuperAdminService>();
            services.AddScoped<ICafeService, CafeService>();
            services.AddScoped<IMenuCategoryService, MenuCategoryService>();
            services.AddScoped<IMenuItemService, MenuItemService>();

            return services;
        }
    }
}

