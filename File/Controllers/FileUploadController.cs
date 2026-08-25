using Application.Common.Dto.Result;
using Application.Services.Filing.FileSrv.Dto;
using Application.Services.Filing.FileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace File.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private static readonly string[] AllowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif",
            ".mp4", ".webm", ".mov",
            ".pdf"
        };

        private static readonly string[] AllowedContentTypePrefixes = { "image/", "video/" };
        private const string AllowedPdfContentType = "application/pdf";

        private readonly IFileService fileService;
        public FileUploadController(IFileService fileService)
        {
            this.fileService = fileService;
        }

        [HttpPost]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null || file.Length <= 0)
                return Ok(new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess));

            var extentionCheck = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var contentType = (file.ContentType ?? string.Empty).ToLowerInvariant();
            var contentTypeAllowed = contentType == AllowedPdfContentType
                || AllowedContentTypePrefixes.Any(prefix => contentType.StartsWith(prefix));

            if (string.IsNullOrWhiteSpace(extentionCheck)
                || !AllowedExtensions.Contains(extentionCheck)
                || !contentTypeAllowed)
                return Ok(new BaseResultDto(isSuccess: false, val: Resource.Notification.FileNotAllow));

            var now = DateTime.Now;
            var extention = extentionCheck;
            var fileName = Guid.NewGuid().ToString("N") + extention;

            string filePath = Path.Combine("wwwroot", "StaticFile", now.Year.ToString(), now.Month.ToString(), now.Day.ToString());
            Directory.CreateDirectory(filePath);

            await using (var stream = System.IO.File.Create(Path.Combine(filePath, fileName)))
            {
                await file.CopyToAsync(stream);
            }

            FileDto fileDto = new FileDto()
            {
                Size = file.Length,
                ContentType = file.ContentType,
                CreateDate = now,
                Extension = extention,
                Name = fileName,
                Url = filePath.Replace("wwwroot", "").Replace("\\", "/")
            };

            var result = await fileService.InsertAsyncDto(fileDto);
            return Ok(result);
        }
    }
}
