using Business.Authentication;
using Data.Repositories;
using Domain.Models;
using Domain.Controller.Private.ScrutinySlate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Domain.API;

namespace API.Controllers.Private
{
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    [Authorize]
    [ApiController]
    [Route("api/scrutinies/{scrutinyId}/slates")]
    public class ScrutinySlateController(ScrutinyRepository scrutinyRepository, SlateRepository slateRepository, SlateCandidacyRepository slateCandidacyRepository) : ControllerBase
    {
        private readonly ScrutinyRepository _scrutinyRepository = scrutinyRepository;
        private readonly SlateRepository _slateRepository = slateRepository;
        private readonly SlateCandidacyRepository _slateCandidacyRepository = slateCandidacyRepository;

        [HttpGet("{id}")]
        [ProducesResponseType<GetByIdResponse.Response>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener informaicón de una plancha.",
            Description = "Devuelve la información de una plancha identificado por su id."
        )]
        public async Task<IActionResult> GetById(Guid scrutinyId, Guid id)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(scrutinyId);
            var dbSlate = await _slateRepository.GetOneByFilter(slate => slate.ScrutinyId == scrutinyId && slate.Id == id);

            if (dbScrutiny == null || dbSlate == null)
            {
                return Ok(new GetByIdResponse.Response
                {
                    Data = null
                });
            }

            var dbSlateCandidacies = await _slateCandidacyRepository.GetAll(slateCandidacy => slateCandidacy.SlateId == dbSlate.Id, "CandidacyType");
            
            var candidacies = dbSlateCandidacies.Data.Select(candidacy => new GetByIdResponse.CandidacyData
            {
                Id = candidacy.Id,
                ScrutinyId = dbSlate.ScrutinyId,
                SlateId = candidacy.SlateId,
                CandidacyTypeId = candidacy.CandidacyTypeId,
                Name = candidacy.Name,
                LastName = candidacy.LastName,
                ImageUrl = candidacy.ImageUrl,
                CandidacyType = new()
                {
                    Id = candidacy.CandidacyType.Id,
                    Name = candidacy.CandidacyType.Name
                }
            });

            return Ok(new GetByIdResponse.Response
            {
                Data = new()
                {
                    Id = dbSlate.Id,
                    ScrutinyId = dbSlate.ScrutinyId,
                    Position = dbSlate.Position,
                    Candidacies = [.. candidacies]
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<GetPaginationResponse.Response>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtener una lista de planchas",
            Description = "Devuelve una lista de todas las planchas pertenecientes a un escrutinio."
        )]
        public async Task<IActionResult> GetPagination(Guid scrutinyId, [FromQuery] PaginationQueryParams query)
        {
            try
            {
                var result = await _slateRepository.GetAll(
                    slate => slate.ScrutinyId == scrutinyId,
                    ["SlateCandidacies"],
                    orderBy => orderBy.Position,
                    pageArg: query.Page,
                    pageSizeArg: query.PageSize
                );

                return Ok(new GetPaginationResponse.Response
                {
                    Pagination = result.Pagination,
                    Data = [.. result.Data.Select(slate => new GetPaginationResponse.Data()
                    {
                        Id = slate.Id,
                        ScrutinyId = slate.ScrutinyId,
                        Position = slate.Position,
                        CountCandidacies = slate.SlateCandidacies.Count,
                        FirstCandidacy = slate.SlateCandidacies
                            .OrderBy(candidacy => candidacy.CandidacyTypeId)
                            .Select(candidacy => new GetPaginationResponse.FirstCandidacyData
                            {
                                Id = candidacy.Id,
                                ScrutinyId = slate.ScrutinyId,
                                SlateId = candidacy.SlateId,
                                CandidacyTypeId = candidacy.CandidacyTypeId,
                                Name = candidacy.Name,
                                LastName = candidacy.LastName
                            })
                            .FirstOrDefault()
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
            Summary = "Agregar una nueva plancha",
            Description = "Crea un nueva plancha a un escrutinio."
        )]
        public async Task<IActionResult> Add(Guid scrutinyId, [FromBody] AddDto data)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(scrutinyId);

            if (dbScrutiny == null) {
                return BadRequest(new BadRequestResponse {
                    BadMessage = "No se ha encontrado el escrutinio"
                });
            }
            else if (dbScrutiny.StatusId != EScrutinyStatus.PENDING.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "El escrutinio ya no se encuentra en estado pendiente. No se puede agregar una plancha."
                });
            }

            var dbSlateByPosition = await _slateRepository.GetOneByFilter(slate => slate.ScrutinyId == scrutinyId && slate.Position == data.Position);

            if (dbSlateByPosition != null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = $"Ya existe una plancha con la posición \"{data.Position}\"."
                });
            }

            var newSlate = await _slateRepository.Create(new()
            {
                ScrutinyId = scrutinyId,
                Position = data.Position,
            });

            return Ok(new AddResponse
            {
                Id = newSlate.Id
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Actualizar una plancha",
            Description = "Actualiza una plancha existente en un escrutinio."
        )]
        public async Task<IActionResult> Update(Guid scrutinyId, Guid id, [FromBody] UpdateDto data)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(scrutinyId);

            if (dbScrutiny == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado el escrutinio."
                });
            }
            else if (dbScrutiny.StatusId != EScrutinyStatus.PENDING.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "El escrutinio ya no se encuentra en estado pendiente. No se puede actualizar la plancha."
                });
            }

            var dbSlate = await _slateRepository.GetOneByFilter(slate => slate.ScrutinyId == scrutinyId && slate.Id == id);

            if (dbSlate == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se encontrado la plancha."
                });
            }

            dbSlate.Position = data.Position ?? dbSlate.Position;

            await _slateRepository.Edit(dbSlate);

            return Ok();
        }
    }
}
