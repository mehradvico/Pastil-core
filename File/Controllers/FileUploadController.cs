using Application.Common.Dto.Result;
using Application.Services.Filing.FileSrv.Dto;
using Application.Services.Filing.FileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace File.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class FileUploadController : ControllerBase
    {
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

            var now = DateTime.Now;
            var extention = Path.GetExtension(file.FileName);
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
