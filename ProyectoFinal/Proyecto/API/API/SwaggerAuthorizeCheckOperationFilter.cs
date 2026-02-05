using Business.Authentication;
using Domain.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API
{
    public class SwaggerAuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // var authorizeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>() ?? [];
            var authorizeAttributes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Concat(
                context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>() ?? []
            )
            .ToList();

            var controllerHasAuthorize = authorizeAttributes.Any();
            var methodHasAuthorize = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();
            var methodHasAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
            var hasAuthorize = ((controllerHasAuthorize == true && controllerHasAuthorize == true) || methodHasAuthorize) && !methodHasAnonymous;

            if (hasAuthorize)
            {
                var schemes = authorizeAttributes
                .SelectMany(attr =>
                    (attr.AuthenticationSchemes ?? AuthScheme.User.ToSchemeName())
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                )
                .Distinct();

                foreach (var scheme in schemes)
                {
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = scheme
                            }
                        }] = []
                    });
                }
            }
        }
    }
}
