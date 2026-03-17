using Business.Authentication;
using Business.Utils;
using Domain.API;
using Domain.Controller.Private.User;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    [Authorize]
    [ApiController]
    [Route("api/user-roles")]
    public class UserRoleController() : ControllerBase
    {
        [HttpGet()]
        [ProducesResponseType<EnumResponse<EUserRole>>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener roles de usuarios.",
            Description = "Devuelve todos los roles de usuarios permitidos en el sistema."
        )]
        public IActionResult Get()
        {
            var data = EUserRoleExtensions.GetList().Select(x => new EnumResponse<EUserRole>.DataResponse
            {
                Value = (EUserRole)EUserRoleExtensions.FromValue(x.Value),
                Name = x.Name,
            }).ToArray();

            return Ok(new EnumResponse<EUserRole>
            {
                Ok = true,
                Data = data
            });
        }
    }
}
