using Data.Repositories;
using Domain.API;
using Domain.Controller.Public.CandidacyType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Public
{
    [ApiController]
    [Route("api/public/candidacy-types")]
    public class CandidacyTypeController(CandidacyTypeRepository candidacyTypeRepository) : ControllerBase
    {
        private readonly CandidacyTypeRepository _candidacyTypeRepository = candidacyTypeRepository;

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType<GetPaginationResponse.Response>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtener una lista paginada de tipos de candidatura",
            Description = "Devuelve una lista paginada de todos los tipos de candidatura registrados en el sistema. Endpoint público, no requiere autenticación."
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
    }
}
