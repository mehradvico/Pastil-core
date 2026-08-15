using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// درخواست ثبت فروشگاه توسط کاربر
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly ICurrentUserHelper _currentUser;

        public StoreController(IStoreService storeService, ICurrentUserHelper currentUser)
        {
            _storeService = storeService;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(StoreSearchDto), 200)]
        public IActionResult Get([FromQuery] StoreInputDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            return Ok(_storeService.Search(dto));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<StoreVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _storeService.FindRequestAsync(id, _currentUser.CurrentUser.UserId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<StoreDto>), 200)]
        public async Task<IActionResult> Post(StoreDto dto)
        {
            return Ok(await _storeService.InsertRequestAsync(dto, _currentUser.CurrentUser.UserId));
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(StoreDto dto)
        {
            return Ok(await _storeService.ResubmitRequestAsync(dto, _currentUser.CurrentUser.UserId));
        }
    }
}
