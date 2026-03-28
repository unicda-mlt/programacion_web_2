using Business.Controllers;
using Data.Repositories;
using Domain.Authentication;
using Domain.Controller.Private.Auth;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [Authorize]
    [ApiController]
    [Route("api/auth")]
    public class AuthController(
        AuthService authService,
        CurrentUserContext userContext,
        StudentRepository studentRepository
    ) : ControllerBase
    {
        private readonly AuthService _authService = authService;
        private readonly CurrentUserContext _userContext = userContext;
        private readonly StudentRepository _studentRepository = studentRepository;

        [AllowAnonymous]
        [HttpPost("generate-token")]
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

        [HttpGet("user-info")]
        [ProducesResponseType<GetUserInfoResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener información del usuario",
            Description = "Recupera información sobre el usuario autenticado basado en el token proporcionado. Este endpoint requiere un token Bearer válido en el encabezado Authorization."
        )]
        public async Task<IActionResult> GetInfoUsuario()
        {
            var user = _userContext.User!;
            Guid? studentId = null;

            if (user.UserRole.Id == EUserRole.STUDENT.GetValue())
            {
                var dbStudent = await _studentRepository.GetOneByFilter(student => student.UserId == user.Id);
                studentId = dbStudent?.Id;
            }

            return Ok(new GetUserInfoResponse()
            {
                Id = user.Id,
                StudentId = studentId,
                UserName = user.UserName,
                UserRole = new()
                {
                    Id = user.UserRole.Id,
                    Name = user.UserRole.Name,
                }
            });
        }
    }
}
