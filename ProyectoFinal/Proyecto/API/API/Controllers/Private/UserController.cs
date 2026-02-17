using Business.Authentication;
using Business.Utils;
using Data.Repositories;
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
    [Route("api/[controller]")]
    public class UserController(UserRepository userRepository, StudentRepository studentRepository) : ControllerBase
    {
        private readonly UserRepository _userRepository = userRepository;
        private readonly StudentRepository _studentRepository = studentRepository;

        [AllowAnonymous]
        [HttpPost("Add")]
        [ProducesResponseType<AddResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Agregar un nuevo usuario",
            Description = "Crea un nuevo usuario en el sistema."
        )]
        public async Task<IActionResult> Add([FromBody] AddDto data)
        {
            if (EUserRoleExtensions.FromValue(data.UserRoleId) == null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "El rol de usuario no existe."
                });
            }

            if (data.UserRoleId == EUserRole.STUDENT.GetValue() && data.StudentId == null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "Debe enviarse el id del estudiante cuando el rol de usuario es estudiante."
                });
            }

            if (data.StudentId != null)
            {
                if (data.UserRoleId != EUserRole.STUDENT.GetValue())
                {
                    return BadRequest(new BadRequestResponse()
                    {
                        BadMessage = "El rol de usuario debe ser tipo estudiante cuando se envia el id del estudiante."
                    });
                }

                var dbStudent = await _studentRepository.GetById((Guid)data.StudentId);

                if (dbStudent == null)
                {
                    return BadRequest(new BadRequestResponse()
                    {
                        BadMessage = "El estudiante no existe."
                    });
                }
                else if (dbStudent.UserId != null) {
                    return BadRequest(new BadRequestResponse()
                    {
                        BadMessage = "El estudiante ya se encuentra con un usuario asignado."
                    });
                }
            }

            var dbUserByUserName = await _userRepository.GetOneByFilter(user => user.UserName == data.UserName);

            if (dbUserByUserName != null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = $"Ya existe un usuario con el nombre de usuario \"{data.UserName}\"."
                });
            }

            var newUser = await _userRepository.Create(new()
            {
                UserRoleId = data.UserRoleId,
                UserName = data.UserName,
                Password = PasswordHasher.HashPassword(data.Password),
                Active = data.Active
            });

            if (data.StudentId != null)
            {
                var dbStudent = await _studentRepository.GetById((Guid)data.StudentId);

                if (dbStudent != null)
                {
                    dbStudent.UserId = newUser.Id;
                    await _studentRepository.Edit(dbStudent);
                }
            }

            return Ok(new AddResponse
            {
                Id = newUser.Id
            });
        }
    }
}
