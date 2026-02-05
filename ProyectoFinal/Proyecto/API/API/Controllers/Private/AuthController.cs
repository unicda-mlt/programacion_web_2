using Business.Controllers;
using Domain.Controller.Private.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        private readonly AuthService _authService = authService;

        [AllowAnonymous]
        [HttpPost("GenerateToken")]
        [ProducesResponseType<GenerateTokenResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener token para autenticación de usuario",
            Description = "Genera un token para la autenticación de usuario basado en las credenciales proporcionadas."
        )]
        public async Task<IActionResult> GenerateToken([FromBody] GenerateUserTokenDto data)
        {
            var token = await _authService.GenerateAuthUserToken(data);
            
            if (token != null) {
                return Ok(new { Token = token });
            }

            return Unauthorized();
        }

        [HttpGet("GetInfoUsuario")]
        [ProducesResponseType<GetUserInfoResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener información del usuario",
            Description = "Recupera información sobre el usuario autenticado basado en el token proporcionado. Este endpoint requiere un token Bearer válido en el encabezado Authorization."
        )]
        public async Task<IActionResult> GetInfoUsuario()
        {
            string? authHeader = Request.Headers.Authorization;

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized();
            }

            string token = authHeader["Bearer ".Length..].Trim();
            var data = await _authService.GetUserInfoResponse(token);

            if (data == null) {
                return Unauthorized();
            }

            return Ok(data);
        }
    }
}
