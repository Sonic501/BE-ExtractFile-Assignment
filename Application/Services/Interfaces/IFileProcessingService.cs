using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Application.Services.Interfaces
{
    public interface IFileProcessingService
    {
        Task<IActionResult> ExtractTextImages(IFormFile file );
        Task<FileStreamResult> DownloadFile();
        Task<IActionResult> UpperCaseText(IFormFile file);
        Task<IActionResult> ExtractFromPP(IFormFile file);
    }
}
