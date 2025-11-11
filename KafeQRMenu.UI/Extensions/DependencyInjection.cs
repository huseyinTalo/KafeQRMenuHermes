using KafeQRMenu.DataAccess.AppContext;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace KafeQRMenu.UI.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUIServices(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());

            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();


            return services;
        }
    }
}

