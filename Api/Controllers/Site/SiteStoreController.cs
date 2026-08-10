using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Site
{
    /// <summary>
    /// فروشگاه‌های قابل نمایش در سایت معرفی پاستیل
    /// </summary>
    [Route("api/Site/Store")]
    [ApiController]
    [AllowAnonymous]
    public class SiteStoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public SiteStoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] StoreInputDto dto)
        {
            dto.Available = true;
            dto.ShowToSite = true;
            return Ok(_storeService.Search(dto));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _storeService.FindAsyncVDto(id);
            if (!result.IsSuccess || result.Data == null || !result.Data.Active || !result.Data.ShowToSite)
                return Ok(new BaseResultDto<StoreVDto>(false, Resource.Notification.NothingFound, null!));

            return Ok(result);
        }
    }
}
