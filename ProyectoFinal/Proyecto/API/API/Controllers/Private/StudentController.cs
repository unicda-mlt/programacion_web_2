using Business.Authentication;
using Business.Utils;
using Data.Repositories;
using Domain.API;
using Domain.Controller.Private.Student;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController(StudentRepository studentRepository) : ControllerBase
    {
        private readonly StudentRepository _studentRepository = studentRepository;

        [HttpGet("{id}")]
        [ProducesResponseType<GetByIdResponse.Response>(StatusCodes.Status200OK)]
        [SwaggerOperation(
              Summary = "Obtener informaicón de un estudiante.",
              Description = "Devuelve la información de un estudiante identificado por su id."
          )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dbStudent = await _studentRepository.GetById(id);

            if (dbStudent == null)
            {
                return Ok(new GetByIdResponse.Response
                {
                    Data = null
                });
            }

            return Ok(new GetByIdResponse.Response
            {
                Data = new()
                {
                    Id = dbStudent.Id,
                    UserId = dbStudent.UserId,
                    RegistrationNumber = dbStudent.RegistrationNumber,
                    Name = dbStudent.Name,
                    LastName = dbStudent.LastName,
                    Graduated = dbStudent.Graduated,
                    CreatedAt = dbStudent.CreatedAt,
                    UpdatedAt = dbStudent.UpdatedAt
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<GetPaginationResponse.Response>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
              Summary = "Obtener una lista de estudiantes",
              Description = "Devuelve una lista de todos los estudiantes registrados en el sistema."
          )]
        public async Task<IActionResult> GetPagination([FromQuery] PaginationQueryParams query)
        {
            try
            {
                var result = await _studentRepository.GetAll(
                    pageArg: query.Page,
                    pageSizeArg: query.PageSize
                );

                return Ok(new GetPaginationResponse.Response
                {
                    Pagination = result.Pagination,
                    Data = [.. result.Data.Select(x => new GetPaginationResponse.Data()
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        RegistrationNumber = x.RegistrationNumber,
                        Name = x.Name,
                        LastName = x.LastName,
                        Graduated = x.Graduated
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
            Summary = "Agregar un nuevo estudiante",
            Description = "Crea un nuevo estudiante en el sistema."
        )]
        public async Task<IActionResult> Add([FromBody] AddDto data)
        {
            var registrationNumber = await StudentIdRegistrationNumberGenerator.GetNewRegistrationNumber(_studentRepository);

            var user = await _studentRepository.Create(new()
            {
                RegistrationNumber = registrationNumber,
                Name = data.Name,
                LastName = data.LastName,
                Graduated = false
            });

            return Ok(new AddResponse
            {
                Id = user.Id
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Editar información de estudiante",
            Description = "Edita la información de un estudiante identificado por su id."
        )]
        public async Task<IActionResult> Update(Guid id, [FromBody] EditDto data)
        {
            var dbStudent = await _studentRepository.GetById(id);

            if (dbStudent == null) {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "El usuario no se ha encontrado."
                });
            }

            dbStudent.Name = data.Name ?? dbStudent.Name;
            dbStudent.LastName = data.LastName ?? dbStudent.LastName;
            dbStudent.Graduated = data.Graduated ?? dbStudent.Graduated;

            await _studentRepository.Edit(dbStudent);

            return Ok(new OkResponse());
        }
    }
}
