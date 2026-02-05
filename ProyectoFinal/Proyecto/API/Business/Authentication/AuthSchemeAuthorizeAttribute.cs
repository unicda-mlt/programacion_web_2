
using Domain.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Business.Authentication
{
    public class AuthSchemeAuthorizeAttribute : AuthorizeAttribute
    {
        public AuthSchemeAuthorizeAttribute(params AuthScheme[] schemes)
        {
            AuthenticationSchemes = string.Join(",", schemes.Select(s => s.ToSchemeName()));
        }
    }
}
