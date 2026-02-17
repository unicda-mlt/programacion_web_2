using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Business.Authentication
{
    public class AuthorizeUserRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeUserRoleAttribute(params EUserRole[] roles)
        {
            Roles = string.Join(",", roles.Select(role => role.GetValue().ToString()));
        }
    }
}
