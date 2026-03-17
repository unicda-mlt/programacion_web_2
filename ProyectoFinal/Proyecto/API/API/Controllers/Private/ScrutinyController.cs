using Business.Authentication;
using Data.Repositories;
using Domain.Models;
using Domain.Controller.Private.Scrutiny;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Domain.API;

namespace API.Controllers.Private
{
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    [Authorize]
    [ApiController]
    [Route("api/scrutinies")]
    public class ScrutinyController(ScrutinyRepository scrutinyRepository, SlateRepository slateRepository) : ControllerBase
    {
        private readonly ScrutinyRepository _scrutinyRepository = scrutinyRepository;
        private readonly SlateRepository _slateRepository = slateRepository;

        [HttpGet("{id}")]
        [ProducesResponseType<GetByIdResponse.Response>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener informaicón de un escrutinio.",
            Description = "Devuelve la información de un escrutinio identificado por su id."
        )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(id, "ScrutinyStatus");

            if (dbScrutiny == null)
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
                    Id = dbScrutiny.Id,
                    StatusId = dbScrutiny.StatusId,
                    Title = dbScrutiny.Title,
                    Description = dbScrutiny.Description,
                    StartDate = dbScrutiny.StartDate,
                    EndDate = dbScrutiny.EndDate,
                    CreatedAt = dbScrutiny.CreatedAt,
                    UpdatedAt = dbScrutiny.UpdatedAt,
                    Status = new()
                    {
                        Id = dbScrutiny.ScrutinyStatus.Id,
                        Name = dbScrutiny.ScrutinyStatus.Name
                    }
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<GetPaginationResponse.Response>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtener una lista de escrutinios",
            Description = "Devuelve una lista de todos los escrutinios registrados en el sistema."
        )]
        public async Task<IActionResult> GetPagination([FromQuery] GetPaginationQuery query)
        {
            try
            {
                if (query.FromDate != null)
                {
                    query.FromDate = query.FromDate.Value.Date;
                }

                if (query.ToDate != null) {
                    query.ToDate = query.ToDate.Value.Date.AddDays(1);
                }

                var result = await _scrutinyRepository.GetAll(
                    scrutiny => (
                        (query.StatusId == null || scrutiny.StatusId == query.StatusId)
                        && (query.FromDate == null || scrutiny.StartDate >= query.FromDate)
                        && (query.ToDate == null || scrutiny.EndDate <= query.ToDate)
                    ),
                    pageArg: query.Page,
                    pageSizeArg: query.PageSize
                );

                return Ok(new GetPaginationResponse.Response
                {
                    Pagination = result.Pagination,
                    Data = [.. result.Data.Select(scrutiny => new GetPaginationResponse.Data()
                    {
                        Id = scrutiny.Id,
                        StatusId = scrutiny.StatusId,
                        Title = scrutiny.Title,
                        StartDate = scrutiny.StartDate,
                        EndDate = scrutiny.EndDate
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
            Summary = "Agregar un nuevo escrutinio",
            Description = "Crea un nuevo escrutinio en el sistema."
        )]
        public async Task<IActionResult> Add([FromBody] AddDto data)
        {
            var newScrutiny = await _scrutinyRepository.Create(new()
            {
                StatusId = EScrutinyStatus.PENDING.GetValue(),
                Title = data.Title,
                Description = data.Description,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
            });

            return Ok(new AddResponse
            {
                Id = newScrutiny.Id
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Actualizar un escrutinio",
            Description = "Actualiza un escrutinio existente en el sistema."
        )]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDto data)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(id);

            if (dbScrutiny == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se encontrado el escrutinio."
                });
            }
            else if (dbScrutiny.StatusId != EScrutinyStatus.PENDING.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "El escrutinio ya no se encuentra en estado pendiente. No se puede actualizar."
                });
            }

            var dbSlates = await _slateRepository.GetAll(slate => slate.ScrutinyId == id);
            var countSlates = dbSlates?.Data.Count ?? 0;

            if (data.StatusId != null && data.StatusId != EScrutinyStatus.PENDING.GetValue() && countSlates < 2)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Debe de haber mínimo 2 planchas para aperturar, cerrar o firmar un escrutinio."
                });
            }

            dbScrutiny.StatusId = data.StatusId ?? dbScrutiny.StatusId;
            dbScrutiny.Title = data.Title ?? dbScrutiny.Title;
            dbScrutiny.Description = data.Description ?? dbScrutiny.Description;
            dbScrutiny.StartDate = data.StartDate ?? dbScrutiny.StartDate;
            dbScrutiny.EndDate = data.EndDate ?? dbScrutiny.EndDate;

            return Ok();
        }
    }
}
