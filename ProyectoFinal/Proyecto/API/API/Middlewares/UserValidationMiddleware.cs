using Azure.Core;
using Business.Authentication;
using Business.Controllers;
using Data;
using Domain.Authentication;
using System.Security.Claims;

namespace API.Middlewares
{
    public class UserValidationMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, AuthService authService, CurrentUserContext userContext)
        {
            string? authHeader = context.Request.Headers.Authorization;

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            string token = authHeader["Bearer ".Length..].Trim();
            var data = await authService.GetUser(token);

            if (data == null || !data.Active || data.UserRole.Id != data.TokenInfo.UserRoleId)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            userContext.User = data;

            await next(context);
        }
    }
}
