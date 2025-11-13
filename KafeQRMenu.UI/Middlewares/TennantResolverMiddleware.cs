using KafeQRMenu.BLogic.Services.CafeServices;

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
            context.Items["Cafe"] = result.Data;

        await _next(context);
    }
}