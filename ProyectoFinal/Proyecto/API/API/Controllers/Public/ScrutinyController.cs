using Business.Authentication;
using Data.Repositories;
using Domain.API;
using Domain.Authentication;
using Domain.Controller.Public.Scrutiny;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Public
{
    [ApiController]
    [Route("api/public/scrutinies")]
    public class ScrutinyController(
        ScrutinyRepository scrutinyRepository,
        SlateRepository slateRepository,
        VoteRepository voteRepository,
        StudentRepository studentRepository,
        CurrentUserContext userContext
    ) : ControllerBase
    {
        private readonly ScrutinyRepository _scrutinyRepository = scrutinyRepository;
        private readonly SlateRepository _slateRepository = slateRepository;
        private readonly VoteRepository _voteRepository = voteRepository;
        private readonly StudentRepository _studentRepository = studentRepository;
        private readonly CurrentUserContext _userContext = userContext;

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType<GetByIdResponse.Response>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Obtener informaicón de un escrutinio.",
            Description = "Devuelve la información de un escrutinio identificado por su id. El escrutinio debe estar en estado abierto."
        )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dbScrutiny = await _scrutinyRepository.GetOneByFilter(scrutiny => scrutiny.Id == id);

            if (dbScrutiny == null)
            {
                return Ok(new BadRequestResponse
                {
                    BadMessage = "No se ha encontrado el escrutinio"
                });
            }

            var canVote = await _scrutinyRepository.CanVote(id);

            if (!canVote)
            {
                return Ok(new BadRequestResponse
                {
                    BadMessage = "El escrutinio no se encuentra disponible para votar"
                });
            }

            var dbSlates = await _slateRepository.GetAllNoPagination(slate => slate.ScrutinyId == id, "SlateCandidacies");

            return Ok(new GetByIdResponse.Response
            {
                Data = new()
                {
                    Id = dbScrutiny.Id,
                    Title = dbScrutiny.Title,
                    Description = dbScrutiny.Description,
                    StartDate = dbScrutiny.StartDate,
                    EndDate = dbScrutiny.EndDate,
                    ImageUrl = dbScrutiny.ImageUrl,
                    Slates = [.. dbSlates.Select(slate => new GetByIdResponse.SlateData()
                    {
                        Id = slate.Id,
                        ScrutinyId = slate.ScrutinyId,
                        Position = slate.Position,
                        Candidacies = [.. slate.SlateCandidacies.Select(candidacy => new GetByIdResponse.CandidacyData()
                        {
                            SlateId = candidacy.SlateId,
                            CandidacyTypeId = candidacy.CandidacyTypeId,
                            Name = candidacy.Name,
                            LastName = candidacy.LastName,
                            ImageUrl = candidacy.ImageUrl
                        })]
                    })]
                }
            });
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType<GetPaginationResponse.Response>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtener una lista de escrutinios",
            Description = "Devuelve una lista de todos los escrutinios que esten abiertos para votar."
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
                        scrutiny.StatusId == EScrutinyStatus.OPEN.GetValue()
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
                        Title = scrutiny.Title,
                        Description = scrutiny.Description,
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

        [HttpPost("{id}/vote")]
        [Authorize]
        [AuthorizeUserRoleAttribute(EUserRole.STUDENT)]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Votar por una plancha.",
            Description = "Vota por la plancha de un escrutinio abierto."
        )]
        public async Task<IActionResult> Vote(Guid id, [FromBody] PostVoteBody body)
        {
            var user = _userContext.User!;
            var canVote = await _scrutinyRepository.CanVote(id);

            if (!canVote)
            {
                return Ok(new BadRequestResponse
                {
                    BadMessage = "El escrutinio no se encuentra disponible para votar"
                });
            }

            var dbStudent = await _studentRepository.GetOneByFilter(student => student.UserId == user.Id);

            if (dbStudent == null)
            {
                return Ok(new BadRequestResponse
                {
                    BadMessage = "Estudiante no encontrado"
                });
            }

            var dbVote = await _voteRepository.GetOneByFilter(vote => vote.ScrutinyId == id && vote.StudentId == dbStudent.Id);

            if (dbVote != null)
            {
                return Ok(new BadRequestResponse
                {
                    BadMessage = "Ya ha ejercido su voto"
                });
            }

            var dbSlate = await _slateRepository.GetOneByFilter(slate => slate.Id == body.SlateId && slate.ScrutinyId == id);

            if (dbSlate == null)
            {
                return Ok(new BadRequestResponse
                {
                    BadMessage = "Plancha no encontrada"
                });
            }

            await _voteRepository.Create(new()
            {
                ScrutinyId = id,
                SlateId = dbSlate.Id,
                UserId = user.Id,
                StudentId = dbStudent.Id,
                IssueDate = DateTime.Now
            });

            return Ok(new OkResponse());
        }
    }
}
