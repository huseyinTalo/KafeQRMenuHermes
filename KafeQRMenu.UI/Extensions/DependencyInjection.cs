using KafeQRMenu.DataAccess.AppContext;
using Microsoft.AspNetCore.Identity;

namespace KafeQRMenu.UI.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUIServices(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();


            return services;
        }
    }
}

