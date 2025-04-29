using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TrustTrade.Services.Web.Interfaces;

public class SuspensionMiddleware
{
    private readonly RequestDelegate _next;

    public SuspensionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        var path = context.Request.Path.ToString().ToLower();

        // Allow static files and support/logout paths
        if (path.Contains("/support/contactsupport") || path.Contains("/account/logout"))
        {
            await _next(context);
            return;
        }
        if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
            var user = await userService.GetCurrentUserAsync(context.User);

            if (user != null && user.Is_Suspended == true)
            {
                context.Response.Redirect("/support/contactsupport?suspended=true");
                return;
            }
        }

        await _next(context);
    }
}
