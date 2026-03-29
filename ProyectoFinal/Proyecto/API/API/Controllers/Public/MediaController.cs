using Business.Services;
using Domain.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Public
{
    [ApiController]
    [Route("api/media")]
    public class MediaController(UploadHandler uploadHandler) : ControllerBase
    {
        private readonly UploadHandler _uploadHandler = uploadHandler;

        [AllowAnonymous]
        [HttpGet("{filePath}")]
        [SwaggerOperation(
            Summary = "Obtener un archivo multimedia.",
            Description = "Sirve el archivo en línea con el tipo de contenido (MIME) correspondiente. No fuerza descarga. Recomendado para mostrar imágenes directamente en el navegador. Acepta el path codificado en URL."
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

        [AllowAnonymous]
        [HttpGet("download/{filePath}")]
        [SwaggerOperation(
            Summary = "Descargar un archivo multimedia.",
            Description = "Fuerza la descarga del archivo con Content-Disposition: attachment, usando el nombre real del archivo. Acepta el path codificado en URL."
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
