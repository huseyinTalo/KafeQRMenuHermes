using KafeQRMenu.DataAccess.AppContext;
using KafeQRMenu.DataAccess.Repositories.AdminRepositories;
using KafeQRMenu.DataAccess.Repositories.CafeRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuCategoryRepositories;
using KafeQRMenu.DataAccess.Repositories.MenuItemRepositories;
using KafeQRMenu.DataAccess.Repositories.SuperAdminRepositories;
using KafeQRMenu.DataAccess.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafeQRMenu.DataAccess.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseSqlServer(configuration.GetConnectionString("AppConnectionString"));
            });

            //IOC
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
            services.AddScoped<ICafeRepository, CafeRepository>();
            services.AddScoped<IMenuCategoryRepository, MenuCategoryRepository>();
            services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            services.AddHttpContextAccessor();
            //IOC Containers

            //Disable after first starts
            SuperAdminSeed.SeedAsync(configuration).GetAwaiter().GetResult();

            return services;
        }
    }
}
