using Ganss.Xss;
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

            // ✅ HTML Sanitizer Configuration
            services.AddSingleton<IHtmlSanitizer>(provider =>
            {
                var sanitizer = new HtmlSanitizer();

                // İzin verilen HTML etiketleri (Quill'in kullandıkları)
                sanitizer.AllowedTags.Clear();
                sanitizer.AllowedTags.Add("p");
                sanitizer.AllowedTags.Add("br");
                sanitizer.AllowedTags.Add("strong"); // Bold
                sanitizer.AllowedTags.Add("em");     // Italic
                sanitizer.AllowedTags.Add("u");      // Underline
                sanitizer.AllowedTags.Add("s");      // Strikethrough
                sanitizer.AllowedTags.Add("h1");
                sanitizer.AllowedTags.Add("h2");
                sanitizer.AllowedTags.Add("h3");
                sanitizer.AllowedTags.Add("ul");     // Unordered list
                sanitizer.AllowedTags.Add("ol");     // Ordered list
                sanitizer.AllowedTags.Add("li");     // List item
                sanitizer.AllowedTags.Add("span");
                sanitizer.AllowedTags.Add("div");    // Quill wrapper

                // İzin verilen CSS özellikleri (Quill color/alignment için)
                sanitizer.AllowedAttributes.Add("style");
                sanitizer.AllowedAttributes.Add("class"); // Quill classes

                sanitizer.AllowedCssProperties.Clear();
                sanitizer.AllowedCssProperties.Add("color");
                sanitizer.AllowedCssProperties.Add("background-color");
                sanitizer.AllowedCssProperties.Add("text-align");

                // Güvenlik: Tehlikeli protokolleri engelle
                sanitizer.AllowedSchemes.Clear();
                sanitizer.AllowedSchemes.Add("http");
                sanitizer.AllowedSchemes.Add("https");
                sanitizer.AllowedSchemes.Add("mailto");

                return sanitizer;
            });
            return services;
        }
    }
}

