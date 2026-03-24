using Business.Services;
using Domain.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/media")]
    public class MediaController(UploadHandler uploadHandler) : ControllerBase
    {
        private readonly UploadHandler _uploadHandler = uploadHandler;

        [HttpGet("{filePath}")]
        [SwaggerOperation(
            Summary = "Obtener un archivo multimedia.",
            Description = "Este endpoint no descarga el archivo, se recomienda su uso para obtener imagenes."
        )]
        public IActionResult GetImage(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "Path is required."
                });
            }

            string decodedFilePath = Uri.UnescapeDataString(filePath);
            var (exists, fullFilePath) = _uploadHandler.GetFullFilePath(decodedFilePath);
            
            if (!exists) {
                return NotFound();
            }

            var provider = new FileExtensionContentTypeProvider();
            provider.TryGetContentType(fullFilePath, out string? contentType);

            return PhysicalFile(
                physicalPath: fullFilePath,
                contentType: contentType ?? "application/octet-stream",
                enableRangeProcessing: true
            );
        }

        [HttpGet("download/{filePath}")]
        [SwaggerOperation(
            Summary = "Descarga un archivo multimedia.",
            Description = "Descargar un archivo multimedia."
        )]
        public IActionResult DownloadFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest(new BadRequestResponse()
                {
                    BadMessage = "Path is required."
                });
            }

            string decodedFilePath = Uri.UnescapeDataString(filePath);
            var (exists, fullFilePath) = _uploadHandler.GetFullFilePath(decodedFilePath);

            if (!exists)
            {
                return NotFound();
            }

            string fileName = Path.GetFileName(fullFilePath);

            return PhysicalFile(
                physicalPath: fullFilePath,
                contentType: "application/octet-stream",
                fileDownloadName: fileName,
                enableRangeProcessing: true
            );
        }
    }
}
