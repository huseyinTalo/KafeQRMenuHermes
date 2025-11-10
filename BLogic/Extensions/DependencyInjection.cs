using KafeQRMenu.BLogic.Services.AdminServices;
using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.BLogic.Services.ImageServices;
using KafeQRMenu.BLogic.Services.MenuCategoryServices;
using KafeQRMenu.BLogic.Services.MenuItemServices;
using KafeQRMenu.BLogic.Services.SuperAdminServices;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KafeQRMenu.BLogic.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBLogicServices(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());

            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ISuperAdminService, SuperAdminService>();
            services.AddScoped<ICafeService, CafeService>();
            services.AddScoped<IMenuCategoryService, MenuCategoryService>();
            services.AddScoped<IMenuItemService, MenuItemService>();
            services.AddScoped<IImageFileService, ImageFileService>();

            return services;
        }
    }
}

