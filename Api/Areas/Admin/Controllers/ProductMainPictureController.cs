using Application.Common.Dto.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تنظیم تصویر اصلی محصول (ایمپورت گروهی عکس محصولات در پنل).
    /// پیش‌فرض: فقط وقتی PictureId خالی است نوشته می‌شود و عکس موجود را عوض نمی‌کند.
    /// فقط با Replace=true (انتخاب صریح ادمین در پنل) عکس موجود جایگزین می‌شود؛ عکس قبلی از سرویس فایل پاک نمی‌شود.
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductMainPictureController : ControllerBase
    {
        private readonly IDataBaseContext _context;

        public ProductMainPictureController(IDataBaseContext context)
        {
            _context = context;
        }

        public class SetMainPictureDto
        {
            public long ProductId { get; set; }
            public long PictureId { get; set; }

            /// <summary>true = اگر محصول از قبل تصویر اصلی دارد، جایگزینش شود (پیش‌فرض false)</summary>
            public bool Replace { get; set; }
        }

        public class SetMainPictureResultDto
        {
            /// <summary>true = تصویر اصلی ثبت (یا جایگزین) شد؛ false = محصول از قبل تصویر اصلی داشت و Replace نبود، پس دست‌نخورده ماند</summary>
            public bool Applied { get; set; }
        }

        /// <summary>
        /// ثبت تصویر اصلی (بدون Replace فقط اگر محصول تصویر اصلی ندارد)
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<SetMainPictureResultDto>), 200)]
        public async Task<IActionResult> Put([FromBody] SetMainPictureDto dto)
        {
            if (dto == null || dto.ProductId <= 0 || dto.PictureId <= 0)
                return Ok(new BaseResultDto<SetMainPictureResultDto>(false, Resource.Notification.InvalidData, null));

            if (!await _context.Pictures.AsNoTracking().AnyAsync(p => p.Id == dto.PictureId))
                return Ok(new BaseResultDto<SetMainPictureResultDto>(false, Resource.Notification.InvalidData, null));

            var now = DateTime.Now;
            var replace = dto.Replace;
            var changed = await _context.Products
                .Where(p => p.Id == dto.ProductId && !p.Deleted && (replace || p.PictureId == null))
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.PictureId, dto.PictureId).SetProperty(p => p.UpdateDate, now));

            if (changed == 0 && !await _context.Products.AsNoTracking().AnyAsync(p => p.Id == dto.ProductId && !p.Deleted))
                return Ok(new BaseResultDto<SetMainPictureResultDto>(false, Resource.Notification.NothingFound, null));

            return Ok(new BaseResultDto<SetMainPictureResultDto>(true, new SetMainPictureResultDto { Applied = changed > 0 }));
        }
    }
}
