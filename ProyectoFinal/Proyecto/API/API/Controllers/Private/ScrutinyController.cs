using Business.Authentication;
using Business.Services;
using Data.Repositories;
using Domain.API;
using Domain.Controller.Private.Scrutiny;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    [Authorize]
    [ApiController]
    [Route("api/scrutinies")]
    public class ScrutinyController(UploadHandler uploadHandler,
        ScrutinyRepository scrutinyRepository,
        SlateRepository slateRepository,
        SlateCandidacyRepository slateCandidacyRepository,
        ScrutinySignRepository scrutinySignRepository,
        VoteRepository voteRepository
    ) : ControllerBase
    {
        private readonly string _uploadImageSubFolder = "scrutinies/images";
        private readonly string _uploadSignSubFolder = "scrutinies/signs";
        private readonly UploadHandler _uploadHandler = uploadHandler;
        private readonly ScrutinyRepository _scrutinyRepository = scrutinyRepository;
        private readonly SlateRepository _slateRepository = slateRepository;
        private readonly SlateCandidacyRepository _slateCandidacyRepository = slateCandidacyRepository;
        private readonly ScrutinySignRepository _scrutinySignRepository = scrutinySignRepository;
        private readonly VoteRepository _voteRepository = voteRepository;

        [HttpGet("{id}")]
        [ProducesResponseType<GetByIdResponse.Response>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener información de un escrutinio.",
            Description = "Devuelve la información de un escrutinio identificado por su id, incluyendo su estado y, si fue firmado, la URL del archivo de firma."
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

            var dbScrutinySign = await _scrutinySignRepository.GetOneByFilter(sign => sign.ScrutinyId  == id);

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
                    ImageUrl = dbScrutiny.ImageUrl,
                    SignFileUrl = dbScrutinySign?.FileUrl,
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
            Summary = "Obtener una lista paginada de escrutinios",
            Description = "Devuelve una lista paginada de escrutinios. Permite filtrar opcionalmente por estado (statusId), fecha de inicio desde (fromDate) y fecha de fin hasta (toDate)."
        )]
        public async Task<IActionResult> GetPagination([FromQuery] GetPaginationQuery query)
        {
            try
            {
                if (query.FromDate != null)
                {
                    query.FromDate = query.FromDate.Value.Date;
                }

                if (query.ToDate != null)
                {
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
                        EndDate = scrutiny.EndDate,
                        ImageUrl = scrutiny.ImageUrl
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
            Description = "Crea un nuevo escrutinio en el sistema. El escrutinio se registra con estado PENDIENTE automáticamente."
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
            Description = "Actualiza parcialmente los datos de un escrutinio existente. Solo se puede actualizar si el escrutinio se encuentra en estado PENDIENTE."
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

            dbScrutiny.Title = data.Title ?? dbScrutiny.Title;
            dbScrutiny.Description = data.Description ?? dbScrutiny.Description;
            dbScrutiny.StartDate = data.StartDate ?? dbScrutiny.StartDate;
            dbScrutiny.EndDate = data.EndDate ?? dbScrutiny.EndDate;

            await _scrutinyRepository.Edit(dbScrutiny);

            return Ok(new OkResponse());
        }

        [HttpPost("{id}/image")]
        [ProducesResponseType<UploadImageResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Actualizar imagen de portada de un escrutinio",
            Description = "Sube y establece la imagen de portada de un escrutinio. Reemplaza la imagen anterior si existía. Solo se permite cuando el escrutinio está en estado PENDIENTE."
        )]
        public async Task<IActionResult> UpdateImage(Guid id, IFormFile file)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(id);

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
                    BadMessage = "El escrutinio ya no se encuentra en estado pendiente. No se puede actualizar el escrutinio."
                });
            }

            var resultUpload = await _uploadHandler.UploadAsync(file, _uploadImageSubFolder);

            if (!resultUpload.IsSuccess)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = resultUpload.MessageOrFilePath
                });
            }

            if (dbScrutiny.ImageUrl != null)
            {
                _uploadHandler.Remove(dbScrutiny.ImageUrl);
            }

            dbScrutiny.ImageUrl = resultUpload.MessageOrFilePath;

            await _scrutinyRepository.Edit(dbScrutiny);

            return Ok(new UploadImageResponse()
            {
                ImageUrl = resultUpload.MessageOrFilePath
            });
        }

        [HttpPatch("{id}/open")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Aperturar un escrutinio",
            Description = "Cambia el estado del escrutinio de PENDIENTE a ABIERTO. Requiere que el escrutinio tenga al menos 2 planchas registradas."
        )]
        public async Task<IActionResult> Open(Guid id)
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
                    BadMessage = "El escrutinio no se encuentra en estado pendiente. No se puede volver a aperturar."
                });
            }

            var dbSlates = await _slateRepository.GetAll(slate => slate.ScrutinyId == id);
            var countSlates = dbSlates?.Data.Count ?? 0;

            if (countSlates < 2)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Debe de haber mínimo 2 planchas para aperturar un escrutinio."
                });
            }

            dbScrutiny.StatusId = EScrutinyStatus.OPEN.GetValue();

            await _scrutinyRepository.Edit(dbScrutiny);

            return Ok(new OkResponse());
        }

        [HttpPost("{id}/close")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Cerrar un escrutinio",
            Description = "Cambia el estado del escrutinio de ABIERTO a CERRADO. Solo se puede cerrar si el escrutinio se encuentra en estado ABIERTO."
        )]
        public async Task<IActionResult> Close(Guid id)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(id);

            if (dbScrutiny == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado el escrutinio."
                });
            }
            else if (dbScrutiny.StatusId != EScrutinyStatus.OPEN.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "El escrutinio debe de estar en estado abierto para poder cerrarse."
                });
            }

            dbScrutiny.StatusId = EScrutinyStatus.CLOSED.GetValue();

            await _scrutinyRepository.Edit(dbScrutiny);

            return Ok(new OkResponse());
        }

        [HttpPost("{id}/sign")]
        [ProducesResponseType<SignResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Firmar un escrutinio",
            Description = "Sube un archivo de firma y cambia el estado del escrutinio de CERRADO a FIRMADO. Acepta archivos .jpg, .png o .pdf con un tamaño máximo de 2 MB. Solo se puede firmar si el escrutinio está en estado CERRADO."
        )]
        public async Task<IActionResult> Sign(Guid id, IFormFile file)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(id);

            if (dbScrutiny == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado el escrutinio."
                });
            }
            else if (dbScrutiny.StatusId != EScrutinyStatus.CLOSED.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "El escrutinio debe estar en estado cerrado para poder ser firmado."
                });
            }

            var fileSize = 2 * 1024 * 1024;
            var fileExtensions = new string[]{ ".jpg", ".png", ".pdf" };
            var resultUpload = await _uploadHandler.UploadAsync(file, _uploadSignSubFolder, fileSize, fileExtensions);

            if (!resultUpload.IsSuccess)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = resultUpload.MessageOrFilePath
                });
            }

            await _scrutinySignRepository.Create(new()
            {
                ScrutinyId = id,
                FileUrl = resultUpload.MessageOrFilePath
            });

            dbScrutiny.StatusId = EScrutinyStatus.SIGNED.GetValue();

            await _scrutinyRepository.Edit(dbScrutiny);

            return Ok(new SignResponse()
            {
                FileUrl = resultUpload.MessageOrFilePath
            });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Eliminar un escrutinio",
            Description = "Elimina un escrutinio y todos sus datos relacionados (planchas, candidaturas, votos, firmas e imágenes). Solo se puede eliminar si el escrutinio está en estado PENDIENTE."
        )]
        public async Task<IActionResult> Delete(Guid id)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(id);

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
                    BadMessage = "Solo se pueden eliminar escrutinios en estado pendiente."
                });
            }

            await _voteRepository.DeleteWhere(vote => vote.ScrutinyId == id);

            var dbSlates = await _slateRepository.GetAllNoPagination(slate => slate.ScrutinyId == id, null);

            foreach (var slate in dbSlates)
            {
                var dbCandidacies = await _slateCandidacyRepository.GetAllNoPagination(
                    candidacy => candidacy.SlateId == slate.Id,
                    null
                );

                foreach (var candidacy in dbCandidacies)
                {
                    if (candidacy.ImageUrl != null)
                    {
                        _uploadHandler.Remove(candidacy.ImageUrl);
                    }
                }

                await _slateCandidacyRepository.DeleteWhere(candidacy => candidacy.SlateId == slate.Id);
            }

            await _slateRepository.DeleteWhere(slate => slate.ScrutinyId == id);

            var dbSigns = await _scrutinySignRepository.GetAllNoPagination(
                sign => sign.ScrutinyId == id,
                null
            );

            foreach (var sign in dbSigns)
            {
                if (sign.FileUrl != null)
                {
                    _uploadHandler.Remove(sign.FileUrl);
                }
            }

            await _scrutinySignRepository.DeleteWhere(sign => sign.ScrutinyId == id);

            if (dbScrutiny.ImageUrl != null)
            {
                _uploadHandler.Remove(dbScrutiny.ImageUrl);
            }

            await _scrutinyRepository.DeleteById(id);

            return Ok(new OkResponse());
        }

        [HttpPatch("{id}/revert-to-open")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Revertir escrutinio a estado ABIERTO",
            Description = "Cambia el estado del escrutinio de CERRADO a ABIERTO. Solo se puede revertir si el escrutinio está en estado CERRADO."
        )]
        public async Task<IActionResult> RevertToOpen(Guid id)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(id);

            if (dbScrutiny == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado el escrutinio."
                });
            }
            else if (dbScrutiny.StatusId != EScrutinyStatus.CLOSED.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Solo se pueden revertir escrutinios en estado cerrado."
                });
            }

            dbScrutiny.StatusId = EScrutinyStatus.OPEN.GetValue();

            await _scrutinyRepository.Edit(dbScrutiny);

            return Ok(new OkResponse());
        }

        [HttpPatch("{id}/revert-to-pending")]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Revertir escrutinio a estado PENDIENTE",
            Description = "Cambia el estado del escrutinio de ABIERTO a PENDIENTE. Esta acción eliminará todos los votos registrados en el escrutinio. Solo se puede revertir si el escrutinio está en estado ABIERTO."
        )]
        public async Task<IActionResult> RevertToPending(Guid id)
        {
            var dbScrutiny = await _scrutinyRepository.GetById(id);

            if (dbScrutiny == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado el escrutinio."
                });
            }
            else if (dbScrutiny.StatusId != EScrutinyStatus.OPEN.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Solo se pueden revertir escrutinios en estado abierto."
                });
            }

            await _voteRepository.DeleteWhere(vote => vote.ScrutinyId == id);

            dbScrutiny.StatusId = EScrutinyStatus.PENDING.GetValue();

            await _scrutinyRepository.Edit(dbScrutiny);

            return Ok(new OkResponse());
        }
    }
}
