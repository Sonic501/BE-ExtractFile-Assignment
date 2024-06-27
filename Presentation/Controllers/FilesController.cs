using Application.Services.Interfaces;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FilesController : ControllerBase
    {
        private readonly IFileProcessingService _fileProcessingService;
        public FilesController(IFileProcessingService fileProcessingService)
        {
            _fileProcessingService = fileProcessingService;
        }
        [HttpPost]
        [Route("extract-text-and-iamges")]
        public async Task<IActionResult> extractTextIamgesAsync([FromForm] IFormFile file)
        {
            try
            {
                var statusCodeResult = await _fileProcessingService.ExtractTextImages(file);
                if (statusCodeResult != null)
                {
                    return statusCodeResult;
                }
                else
                    return await _fileProcessingService.DownloadFile();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [Route("Uppercase-doc")]
        public async Task<IActionResult> Uppercase([FromForm] IFormFile file)
        {
            try
            {
                return await _fileProcessingService.UpperCaseText(file);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [Route("powerpoint-extract")]
        public async Task<IActionResult> ExtractFromPP([FromForm] IFormFile file)
        {
            try
            {
                return await _fileProcessingService.ExtractFromPP(file);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }
    }
}
