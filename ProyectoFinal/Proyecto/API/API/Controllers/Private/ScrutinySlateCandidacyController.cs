using Business.Authentication;
using Business.Services;
using Data.Repositories;
using Domain.API;
using Domain.Controller.Private.ScrutinySlateCandidacy;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    [Authorize]
    [ApiController]
    [Route("api/scrutinies/{scrutinyId}/slates/{slateId}/candidacies")]
    public class ScrutinySlateCandidacyController(
        UploadHandler uploadHandler,
        ScrutinyRepository scrutinyRepository,
        SlateRepository slateRepository,
        SlateCandidacyRepository slateCandidacyRepository,
        CandidacyTypeRepository candidacyTypeRepository
    ) : ControllerBase
    {
        private readonly string _uploadSubFolder = "candidacies";
        private readonly UploadHandler _uploadHandler = uploadHandler;
        private readonly ScrutinyRepository _scrutinyRepository = scrutinyRepository;
        private readonly SlateRepository _slateRepository = slateRepository;
        private readonly SlateCandidacyRepository _slateCandidacyRepository = slateCandidacyRepository;
        private readonly CandidacyTypeRepository _candidacyTypeRepository = candidacyTypeRepository;

        [HttpGet("{id}")]
        [ProducesResponseType<GetByIdResponse.Response>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener información de una candidatura.",
            Description = "Devuelve la información de una candidatura identificada por su id dentro de una plancha, incluyendo el tipo de candidatura asociado."
        )]
        public async Task<IActionResult> GetById(Guid scrutinyId, Guid slateId, Guid id)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(scrutinyId);
            var dbSlate = await _slateRepository.GetOneByFilter(slate => slate.ScrutinyId == scrutinyId && slate.Id == slateId);

            if (dbScrutiny == null || dbSlate == null)
            {
                return Ok(new GetByIdResponse.Response
                {
                    Data = null
                });
            }

            var dbCandidacy = await _slateCandidacyRepository.GetOneByFilter(
                candidacy => candidacy.SlateId == slateId && candidacy.Id == id,
                "CandidacyType"
            );

            if (dbCandidacy == null)
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
                    Id = dbCandidacy.Id,
                    ScrutinyId = dbSlate.ScrutinyId,
                    SlateId = dbCandidacy.SlateId,
                    CandidacyTypeId = dbCandidacy.CandidacyTypeId,
                    Name = dbCandidacy.Name,
                    LastName = dbCandidacy.LastName,
                    ImageUrl = dbCandidacy.ImageUrl,
                    CandidacyType = new()
                    {
                        Id = dbCandidacy.CandidacyType.Id,
                        Name = dbCandidacy.CandidacyType.Name
                    }
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<GetPaginationResponse.Response>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtener una lista de candidaturas",
            Description = "Devuelve una lista de todas las candidaturas pertenecientes a una plancha de un escrutinio."
        )]
        public async Task<IActionResult> GetPagination(Guid scrutinyId, Guid slateId, [FromQuery] PaginationQueryParams query)
        {
            try
            {
                var dbScrutiny = await _scrutinyRepository.GetById(scrutinyId);
                var dbSlate = await _slateRepository.GetOneByFilter(slate => slate.ScrutinyId == scrutinyId && slate.Id == slateId);

                if (dbScrutiny == null || dbSlate == null)
                {
                    return Ok(new GetPaginationResponse.Response
                    {
                        Pagination = new Pagination(),
                        Data = []
                    });
                }

                var result = await _slateCandidacyRepository.GetAll(
                    candidacy => candidacy.SlateId == slateId,
                    "CandidacyType",
                    pageArg: query.Page,
                    pageSizeArg: query.PageSize
                );

                return Ok(new GetPaginationResponse.Response
                {
                    Pagination = result.Pagination,
                    Data = [.. result.Data.Select(candidacy => new GetPaginationResponse.Data()
                    {
                        Id = candidacy.Id,
                        ScrutinyId = dbSlate.ScrutinyId,
                        SlateId = candidacy.SlateId,
                        CandidacyTypeId = candidacy.CandidacyTypeId,
                        CandidacyTypePosition = candidacy.CandidacyType.Position,
                        Name = candidacy.Name,
                        LastName = candidacy.LastName,
                        ImageUrl = candidacy.ImageUrl
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
            Summary = "Agregar una nueva candidatura",
            Description = "Crea una nueva candidatura en una plancha de un escrutinio. Valida que el escrutinio esté en estado PENDIENTE, que la plancha pertenezca al escrutinio y que el tipo de candidatura exista."
        )]
        public async Task<IActionResult> Add(Guid scrutinyId, Guid slateId, [FromBody] AddDto data)
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
                    BadMessage = "El escrutinio ya no se encuentra en estado pendiente. No se puede agregar una candidatura."
                });
            }

            var dbSlate = await _slateRepository.GetOneByFilter(slate => slate.ScrutinyId == scrutinyId && slate.Id == slateId);

            if (dbSlate == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado la plancha."
                });
            }

            var dbCandidacyType = await _candidacyTypeRepository.GetById(data.CandidacyTypeId);

            if (dbCandidacyType == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado el tipo de candidatura."
                });
            }

            var newCandidacy = await _slateCandidacyRepository.Create(new()
            {
                SlateId = slateId,
                CandidacyTypeId = data.CandidacyTypeId,
                Name = data.Name,
                LastName = data.LastName
            });

            return Ok(new AddResponse
            {
                Id = newCandidacy.Id
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Actualizar una candidatura",
            Description = "Actualiza parcialmente una candidatura existente en una plancha. Solo se permite si el escrutinio está en estado PENDIENTE. Si se envía un nuevo tipo de candidatura, valida que exista."
        )]
        public async Task<IActionResult> Update(Guid scrutinyId, Guid slateId, Guid id, [FromBody] UpdateDto data)
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
                    BadMessage = "El escrutinio ya no se encuentra en estado pendiente. No se puede actualizar la candidatura."
                });
            }

            var dbSlate = await _slateRepository.GetOneByFilter(slate => slate.ScrutinyId == scrutinyId && slate.Id == slateId);

            if (dbSlate == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado la plancha."
                });
            }

            var dbCandidacy = await _slateCandidacyRepository.GetOneByFilter(
                candidacy => candidacy.SlateId == slateId && candidacy.Id == id
            );

            if (dbCandidacy == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado la candidatura."
                });
            }

            var dbCandidacyType = data.CandidacyTypeId != null ? await _candidacyTypeRepository.GetById((short)data.CandidacyTypeId) : null;

            if (data.CandidacyTypeId != null && dbCandidacyType == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado el tipo de candidatura."
                });
            }

            dbCandidacy.CandidacyTypeId = data.CandidacyTypeId ?? dbCandidacy.CandidacyTypeId;
            dbCandidacy.Name = data.Name ?? dbCandidacy.Name;
            dbCandidacy.LastName = data.LastName ?? dbCandidacy.LastName;

            await _slateCandidacyRepository.Edit(dbCandidacy);

            return Ok(new OkResponse());
        }

        [HttpPost("{id}/image")]
        [ProducesResponseType<UploadImageResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Actualizar imagen de una candidatura",
            Description = "Sube y establece la imagen de una candidatura. Reemplaza la imagen anterior si existía. Solo se permite cuando el escrutinio está en estado PENDIENTE."
        )]
        public async Task<IActionResult> UpdateImage(Guid scrutinyId, Guid slateId, Guid id, IFormFile file)
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
                    BadMessage = "El escrutinio ya no se encuentra en estado pendiente. No se puede actualizar la candidatura."
                });
            }

            var dbSlate = await _slateRepository.GetOneByFilter(slate => slate.ScrutinyId == scrutinyId && slate.Id == slateId);

            if (dbSlate == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado la plancha."
                });
            }

            var dbCandidacy = await _slateCandidacyRepository.GetOneByFilter(
                candidacy => candidacy.SlateId == slateId && candidacy.Id == id
            );

            if (dbCandidacy == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado la candidatura."
                });
            }

            var resultUpload = await _uploadHandler.UploadAsync(file, _uploadSubFolder);

            if (!resultUpload.IsSuccess)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = resultUpload.MessageOrFilePath
                });
            }

            if (dbCandidacy.ImageUrl != null)
            {
                _uploadHandler.Remove(dbCandidacy.ImageUrl);
            }

            dbCandidacy.ImageUrl = resultUpload.MessageOrFilePath;

            await _slateCandidacyRepository.Edit(dbCandidacy);

            return Ok(new UploadImageResponse() {
                ImageUrl = resultUpload.MessageOrFilePath
            });
        }
    }
}
