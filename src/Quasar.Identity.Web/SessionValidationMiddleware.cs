using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quasar.Identity.Persistence.Relational.EfCore;
using Quasar.Persistence.Relational.EfCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Quasar.Identity.Web;

/// <summary>
/// Middleware that validates session status on every authenticated request.
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ReadModelContext<IdentityReadModelStore> db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sidClaim = context.User.FindFirst("sid")?.Value;
            if (!string.IsNullOrEmpty(sidClaim) && Guid.TryParse(sidClaim, out var sessionId))
            {
                var session = await db.Set<IdentitySessionReadModel>()
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);

                if (session is null || session.RevokedUtc != null || session.ExpiresUtc < DateTime.UtcNow)
                {
                    var logger = context.RequestServices.GetService<ILogger<SessionValidationMiddleware>>();
                    logger?.LogWarning("SessionValidation: 401. SessionId: {Sid}. Found: {Found}, Revoked: {Revoked}, Expired: {Expired}", 
                        sessionId, session != null, session?.RevokedUtc, session?.ExpiresUtc < DateTime.UtcNow);
                        
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new 
                    { 
                        message = "Your session has been revoked. Please log in again.",
                        code = "SESSION_REVOKED"
                    });
                    return;
                }
            }
        }
        await _next(context);
    }
}
