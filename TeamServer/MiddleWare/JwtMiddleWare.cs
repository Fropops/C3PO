namespace TeamServer.MiddleWare;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TeamServer.Services;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IJwtUtils jwtUtils)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var user = jwtUtils.ValidateToken(token);
        if (user != null)
        {
            // attach user to context on successful jwt validation
            context.Items["User"] = user;
        }

        await _next(context);
    }
}