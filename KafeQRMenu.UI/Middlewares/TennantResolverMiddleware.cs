using KafeQRMenu.BLogic.Services.CafeServices;
using KafeQRMenu.DataAccess.AppContext;

public class CafeTenantMiddleware
{
    private readonly RequestDelegate _next;

    public CafeTenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cafeService = context.RequestServices.GetRequiredService<ICafeService>();
        var host = context.Request.Host.Host;

        // Try exact match first
        var result = await cafeService.GetByDomainAsync(host);

        // If not found and has www, try without www
        if (!result.IsSuccess && host.StartsWith("www."))
        {
            var hostWithoutWww = host.Substring(4);
            result = await cafeService.GetByDomainAsync(hostWithoutWww);
        }
        // If not found and doesn't have www, try with www
        else if (!result.IsSuccess && !host.StartsWith("www."))
        {
            var hostWithWww = $"www.{host}";
            result = await cafeService.GetByDomainAsync(hostWithWww);
        }

        if (result.IsSuccess && result.Data != null)
        {
            // SADECE primitive değerleri sakla, entity'yi değil
            context.Items["CafeId"] = result.Data.Id;
            context.Items["CafeName"] = result.Data.CafeName;
            context.Items["CafeDomain"] = result.Data.DomainName;
            // İhtiyacın olan diğer primitive değerler...
        }
        
        await _next(context);
    }
}