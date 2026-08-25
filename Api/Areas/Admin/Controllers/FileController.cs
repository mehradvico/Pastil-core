using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Filing.FileSrv.Dto;
using Application.Services.Filing.FileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فایل
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        /// <summary>
        /// فایل
        /// </summary>
        ///        
        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }
        /// <summary>
        ///  جستجو 
        /// </summary>

        [HttpGet]
        [ProducesResponseType(typeof(BaseSearchDto<FileVDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto baseInputDto)
        {
            var result = _fileService.Search(baseInputDto);
            return Ok(result);
        }
    }
}
