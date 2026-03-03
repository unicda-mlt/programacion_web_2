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

        [HttpGet("{id}")]
        [ProducesResponseType<GetByIdResponse.Response>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener informaicón de un usuario.",
            Description = "Devuelve la información de un usuario identificado por su id."
        )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dbUser = await _userRepository.GetById(id);

            if (dbUser == null)
            {
                return Ok(new GetByIdResponse.Response
                {
                    Data = null
                });
            }

            Guid? studentId = null;

            if (dbUser.UserRoleId == EUserRole.STUDENT.GetValue())
            {
                var dbStudent = await _studentRepository.GetOneByFilter(x => x.UserId == dbUser.Id);
                studentId = dbStudent?.Id;
            }

            return Ok(new GetByIdResponse.Response
            {
                Data = new()
                {
                    Id = dbUser.Id,
                    UserRoleId = dbUser.UserRoleId,
                    StudentId = studentId,
                    UserName = dbUser.UserName,
                    Active = dbUser.Active,
                    CreatedAt = dbUser.CreatedAt,
                    UpdatedAt = dbUser.UpdatedAt
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<GetPaginationResponse.Response>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtener una lista de usuarios",
            Description = "Devuelve una lista de todos los usuarios registrados en el sistema."
        )]
        public async Task<IActionResult> GetPagination([FromQuery] PaginationQueryParams query)
        {
            try
            {
                var result = await _userRepository.GetAll(
                    pageArg: query.Page,
                    pageSizeArg: query.PageSize
                );

                return Ok(new GetPaginationResponse.Response
                {
                    Pagination = result.Pagination,
                    Data = [.. result.Data.Select(x => new GetPaginationResponse.Data()
                    {
                        Id = x.Id,
                        UserRoleId = x.UserRoleId,
                        UserName = x.UserName,
                        Active = x.Active
                    })]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BadRequestResponse { BadMessage = $"Ocurrió un error interno: {ex.Message}" });
            }
        }

        [HttpPost]
        [ProducesResponseType<AddResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Agregar un nuevo usuario",
            Description = "Crea un nuevo usuario en el sistema."
        )]
        public async Task<IActionResult> Add([FromBody] AddDto data)
        {
            string userName = data.UserName;

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

                userName = dbStudent.RegistrationNumber;
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
                UserName = userName,
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

        [HttpPatch("{id}")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Actualizar un usuario",
            Description = "Actualiza un usuario existente en el sistema."
        )]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDto data)
        {
            var dbUser = await _userRepository.GetById(id);

            if (dbUser == null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "El usuario no existe."
                });
            }

            short userRoleId = data.UserRoleId ?? dbUser.UserRoleId;

            if (EUserRoleExtensions.FromValue(userRoleId) == null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "El rol de usuario no existe."
                });
            }

            var currentStudent = await _studentRepository.GetOneByFilter(student => student.UserId == dbUser.Id);
            string userName = data.UserName ?? dbUser.UserName;
            Guid? targetStudentId = null;
            Student? targetStudent = null;

            if (userRoleId == EUserRole.STUDENT.GetValue())
            {
                targetStudentId = data.StudentId ?? currentStudent?.Id;

                if (targetStudentId == null)
                {
                    return BadRequest(new BadRequestResponse()
                    {
                        BadMessage = "Debe enviarse el id del estudiante cuando el rol de usuario es estudiante."
                    });
                }

                targetStudent = await _studentRepository.GetById((Guid)targetStudentId);

                if (targetStudent == null)
                {
                    return BadRequest(new BadRequestResponse()
                    {
                        BadMessage = "El estudiante no existe."
                    });
                }

                if (targetStudent.UserId != null && targetStudent.UserId != dbUser.Id)
                {
                    return BadRequest(new BadRequestResponse()
                    {
                        BadMessage = "El estudiante ya se encuentra con un usuario asignado."
                    });
                }

                userName = targetStudent.RegistrationNumber;
            }
            else if (data.StudentId != null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "El rol de usuario debe ser tipo estudiante cuando se envia el id del estudiante."
                });
            }

            var dbUserByUserName = await _userRepository.GetOneByFilter(user => user.UserName == userName && user.Id != dbUser.Id);

            if (dbUserByUserName != null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = $"Ya existe un usuario con el nombre de usuario \"{userName}\"."
                });
            }

            if (currentStudent != null && (userRoleId != EUserRole.STUDENT.GetValue() || currentStudent.Id != targetStudentId))
            {
                currentStudent.UserId = null;
                await _studentRepository.Edit(currentStudent);
            }

            if (targetStudent != null && targetStudent.UserId != dbUser.Id)
            {
                targetStudent.UserId = dbUser.Id;
                await _studentRepository.Edit(targetStudent);
            }

            dbUser.UserRoleId = userRoleId;
            dbUser.UserName = userName;
            dbUser.Password = data.Password != null ? PasswordHasher.HashPassword(data.Password) : dbUser.Password;
            dbUser.Active = data.Active ?? dbUser.Active;

            await _userRepository.Edit(dbUser);

            return Ok();
        }
    }
}
