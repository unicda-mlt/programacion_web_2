using Business.Authentication;
using Data.Repositories;
using Domain.API;
using Domain.Controller.Private.CandidacyType;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    [Authorize]
    [ApiController]
    [Route("api/candidacy-types")]
    public class CandidacyTypeController(CandidacyTypeRepository candidacyTypeRepository) : ControllerBase
    {
        private readonly CandidacyTypeRepository _candidacyTypeRepository = candidacyTypeRepository;

        [HttpGet("{id}")]
        [ProducesResponseType<GetByIdResponse.Response>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener información de un tipo de candidatura",
            Description = "Devuelve la información de un tipo de candidatura identificado por su id."
        )]
        public async Task<IActionResult> GetById(short id)
        {
            var dbCandidacyType = await _candidacyTypeRepository.GetById(id);

            if (dbCandidacyType == null)
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
                    Id = dbCandidacyType.Id,
                    Name = dbCandidacyType.Name,
                    Position = dbCandidacyType.Position
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<GetPaginationResponse.Response>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtener una lista de tipos de candidatura",
            Description = "Devuelve una lista de todos los tipos de candidatura registrados en el sistema."
        )]
        public async Task<IActionResult> GetPagination([FromQuery] PaginationQueryParams query)
        {
            try
            {
                var result = await _candidacyTypeRepository.GetAll(
                    pageArg: query.Page,
                    pageSizeArg: query.PageSize
                );

                return Ok(new GetPaginationResponse.Response
                {
                    Pagination = result.Pagination,
                    Data = [.. result.Data.Select(x => new GetPaginationResponse.Data()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Position = x.Position
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
            Summary = "Agregar un nuevo tipo de candidatura",
            Description = "Crea un nuevo tipo de candidatura en el sistema. El nombre y la posición deben ser únicos (el nombre se compara sin distinción de mayúsculas/minúsculas ni espacios)."
        )]
        public async Task<IActionResult> Add([FromBody] AddDto data)
        {
            string trimmedName = data.Name.Trim();

            if (trimmedName.Length == 0)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "El nombre del tipo de candidatura es obligatorio."
                });
            }

            var existsByName = await _candidacyTypeRepository.ExistsByNormalizedName(trimmedName);

            if (existsByName)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = $"Ya existe un tipo de candidatura con el nombre \"{trimmedName}\"."
                });
            }

            var dbByPosition = await _candidacyTypeRepository.GetOneByFilter(x => x.Position == data.Position);

            if (dbByPosition != null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = $"Ya existe un tipo de candidatura con la posición \"{data.Position}\"."
                });
            }

            var newCandidacyType = await _candidacyTypeRepository.Create(new()
            {
                Name = trimmedName,
                Position = data.Position
            });

            return Ok(new AddResponse
            {
                Id = newCandidacyType.Id
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Editar información de tipo de candidatura",
            Description = "Actualiza parcialmente el nombre y/o posición de un tipo de candidatura. Valida que el nombre y la posición sean únicos entre los demás tipos existentes."
        )]
        public async Task<IActionResult> Update(short id, [FromBody] UpdateDto data)
        {
            var dbCandidacyType = await _candidacyTypeRepository.GetById(id);

            if (dbCandidacyType == null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "El tipo de candidatura no se ha encontrado."
                });
            }

            string name = (data.Name ?? dbCandidacyType.Name).Trim();

            if (name.Length == 0)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "El nombre del tipo de candidatura es obligatorio."
                });
            }

            short position = data.Position ?? dbCandidacyType.Position;

            var existsByName = await _candidacyTypeRepository.ExistsByNormalizedName(name, id);

            if (existsByName)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = $"Ya existe un tipo de candidatura con el nombre \"{name}\"."
                });
            }

            var dbByPosition = await _candidacyTypeRepository.GetOneByFilter(x => x.Position == position && x.Id != id);

            if (dbByPosition != null)
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = $"Ya existe un tipo de candidatura con la posición \"{position}\"."
                });
            }

            dbCandidacyType.Name = name;
            dbCandidacyType.Position = position;

            await _candidacyTypeRepository.Edit(dbCandidacyType);

            return Ok(new OkResponse());
        }
    }
}
