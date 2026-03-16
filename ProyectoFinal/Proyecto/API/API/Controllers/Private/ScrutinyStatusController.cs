using Business.Authentication;
using Domain.API;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ScrutinyStatusController() : ControllerBase
    {
        [HttpGet()]
        [ProducesResponseType<EnumResponse<EScrutinyStatus>>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener estados de escrutinios.",
            Description = "Devuelve todos los estados posibles de los escrutinios."
        )]
        public IActionResult Get()
        {
            var data = EScrutinyStatusExtensions.GetList().Select(x => new EnumResponse<EScrutinyStatus>.DataResponse
            {
                Value = (EScrutinyStatus)EScrutinyStatusExtensions.FromValue(x.Value),
                Name = x.Name,
            }).ToArray();

            return Ok(new EnumResponse<EScrutinyStatus>
            {
                Ok = true,
                Data = data
            });
        }
    }
}
