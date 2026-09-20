using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.AiProductMatchSrv;
using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Api.Services.AiProductMatch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// تطبیق هوشمند محصولات با کمک هوش مصنوعی (Gemini) برای درج سریع موجودی فروشگاه
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AiProductMatchController : ControllerBase
    {
        private readonly IAiProductMatchService _aiProductMatchService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IAiProductMatchJobStore _jobStore;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AiProductMatchController> _logger;
        private readonly AiProductMatchExecutionGate _executionGate;

        public AiProductMatchController(
            IAiProductMatchService aiProductMatchService,
            ICurrentUserHelper currentUser,
            IAiProductMatchJobStore jobStore,
            IServiceScopeFactory scopeFactory,
            ILogger<AiProductMatchController> logger,
            AiProductMatchExecutionGate executionGate)
        {
            _aiProductMatchService = aiProductMatchService;
            _currentUser = currentUser;
            _jobStore = jobStore;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _executionGate = executionGate;
        }

        /// <summary>
        /// وضعیت در دسترس بودن سرویس هوش مصنوعی — بدون افشای هیچ بخشی از کلید
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(BaseResultDto<AiProductMatchStatusDto>), 200)]
        public async Task<IActionResult> Status()
        {
            var result = await _aiProductMatchService.GetStatusAsync();
            return Ok(result);
        }

        /// <summary>
        /// تحلیل تصویر قفسه، اسکرین‌شات جدول نرم‌افزار انبار (سپیدار/دامپزشکیار)، یا داده‌ی جدول (Excel/سپیدار/دامپزشکیار) و تطبیق با کاتالوگ پاستیل
        /// </summary>
        [HttpPost("analyze")]
        [EnableRateLimiting("AiProductMatch")]
        // تا ۴۰ اسکرین‌شات جدول (سپیدار/دامپزشکیار) پوشش داده می‌شود، نه فقط ۸ عکس قفسه؛ سقف واقعی هر
        // تصویر همچنان با AiProductMatchOptions.MaxImageSizeBytes کنترل می‌شود.
        [RequestSizeLimit(128 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 128 * 1024 * 1024)]
        [ProducesResponseType(typeof(BaseResultDto<AiProductMatchAnalyzeResultDto>), 200)]
        [ProducesResponseType(typeof(BaseResultDto<AiProductMatchAnalyzeResultDto>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Analyze([FromForm] AiProductMatchAnalyzeInputDto dto)
        {
            var currentUser = _currentUser.CurrentUser;
            if (currentUser is null || currentUser.StoreId <= 0)
                return Forbid();

            using var executionLease = _executionGate.TryAcquire();
            if (executionLease == null)
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new BaseResultDto<AiProductMatchAnalyzeResultDto>(false, "سرویس هوش مصنوعی در حال پردازش درخواست‌های دیگر است. لطفاً کمی بعد دوباره تلاش کنید.", null!, StatusCodes.Status429TooManyRequests));

            var storeId = currentUser.StoreId;
            var result = await _aiProductMatchService.AnalyzeAsync(storeId, dto, HttpContext.RequestAborted);
            return Ok(result);
        }

        /// <summary>
        /// همون آنالیز، ولی async/Job-based: بلافاصله jobId برمی‌گرده و پردازش واقعی در پس‌زمینه انجام
        /// می‌شود — برای سناریوهایی مثل اسکرین‌شات‌های زیاد (سپیدار/دامپزشکیار) که ممکن است طول بکشد و
        /// یک درخواست HTTP همزمان را در معرض Timeout سمت کلاینت بگذارد. اپ فروشنده (قفسه) نیازی به این
        /// مسیر ندارد و همچنان از همون /analyze همزمان استفاده می‌کند.
        /// </summary>
        [HttpPost("analyze/start")]
        [EnableRateLimiting("AiProductMatch")]
        [RequestSizeLimit(128 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 128 * 1024 * 1024)]
        [ProducesResponseType(typeof(BaseResultDto<AiProductMatchJobStartedDto>), 200)]
        [ProducesResponseType(typeof(BaseResultDto<AiProductMatchJobStartedDto>), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> StartAnalyze([FromForm] AiProductMatchAnalyzeInputDto dto, [FromServices] Microsoft.Extensions.Options.IOptions<AiProductMatchOptions> options)
        {
            var currentUser = _currentUser.CurrentUser;
            if (currentUser is null || currentUser.StoreId <= 0)
                return Forbid();

            var executionLease = _executionGate.TryAcquire();
            if (executionLease == null)
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new BaseResultDto<AiProductMatchJobStartedDto>(false, "سرویس هوش مصنوعی در حال پردازش درخواست‌های دیگر است. لطفاً کمی بعد دوباره تلاش کنید.", null!, StatusCodes.Status429TooManyRequests));

            var storeId = currentUser.StoreId;

            try
            {
                // بافر کردن بایت‌های عکس‌ها همین الان لازم است: بعد از برگشتن این پاسخ، استریم فایل‌های
                // آپلودشده‌ی همین درخواست HTTP بسته می‌شود و در Task پس‌زمینه دیگر قابل‌خواندن نیست.
                var bufferedDto = await BufferImagesAsync(dto);

                var sourceType = dto.SourceType?.Trim().ToLowerInvariant();
                var bufferedImages = bufferedDto.Images ?? new List<IFormFile>();
                var isTableCapture = (sourceType == "veterinary" || sourceType == "sepidar") && bufferedImages.Count > 0;
                var totalBatches = isTableCapture
                    ? Math.Max(1, (int)Math.Ceiling(bufferedImages.Count / (double)Math.Max(1, options.Value.TableImagesPerVisionCall)))
                    : Math.Max(1, bufferedImages.Count);

                var job = _jobStore.Create(totalBatches);

                _ = Task.Run(async () =>
                {
                    using (executionLease)
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        // Scope جدید و جدا از Scope همین درخواست HTTP — چون تا زمانی که این Task اجرا می‌شود،
                        // Scope درخواست اصلی (و DbContext داخلش) از قبل Dispose شده.
                        var service = scope.ServiceProvider.GetRequiredService<IAiProductMatchService>();
                        try
                        {
                            var result = await service.AnalyzeAsync(
                                storeId, bufferedDto, System.Threading.CancellationToken.None,
                                (completed, total) => _jobStore.ReportProgress(job.JobId, completed));

                            if (result.IsSuccess)
                                _jobStore.Complete(job.JobId, result.Data);
                            else
                                _jobStore.Fail(job.JobId, result.Messages?.FirstOrDefault()?.Item1, result.Code);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "AiProductMatch background job {JobId} failed unexpectedly for store {StoreId}.", job.JobId, storeId);
                            _jobStore.Fail(job.JobId, "خطای غیرمنتظره در پردازش.", -1);
                        }
                    }
                });

                return Ok(new BaseResultDto<AiProductMatchJobStartedDto>(true, new AiProductMatchJobStartedDto
                {
                    JobId = job.JobId,
                    TotalBatches = job.TotalBatches
                }));
            }
            catch
            {
                executionLease.Dispose();
                throw;
            }
        }

        /// <summary>
        /// وضعیت/پیشرفت یک Job که با /analyze/start شروع شده — تا وقتی status="completed" نشده،
        /// result همیشه null است.
        /// </summary>
        [HttpGet("analyze/status/{jobId}")]
        [ProducesResponseType(typeof(BaseResultDto<AiProductMatchJobStatusDto>), 200)]
        public IActionResult AnalyzeStatus(string jobId)
        {
            var job = _jobStore.Get(jobId);
            if (job == null)
                return Ok(new BaseResultDto<AiProductMatchJobStatusDto>(false, Resource.Notification.AiProductMatchInvalidInput, null, 1));

            return Ok(new BaseResultDto<AiProductMatchJobStatusDto>(true, new AiProductMatchJobStatusDto
            {
                Status = job.Status,
                TotalBatches = job.TotalBatches,
                CompletedBatches = job.CompletedBatches,
                Result = job.Status == "completed" ? job.Result : null
            }));
        }

        private static async Task<AiProductMatchAnalyzeInputDto> BufferImagesAsync(AiProductMatchAnalyzeInputDto dto)
        {
            var buffered = new AiProductMatchAnalyzeInputDto
            {
                SourceType = dto.SourceType,
                RowsJson = dto.RowsJson,
                Currency = dto.Currency,
                Images = new List<IFormFile>()
            };

            if (dto.Images == null)
                return buffered;

            foreach (var image in dto.Images)
            {
                await using var stream = image.OpenReadStream();
                var memory = new MemoryStream();
                await stream.CopyToAsync(memory);
                memory.Position = 0;

                buffered.Images.Add(new FormFile(memory, 0, memory.Length, image.Name, image.FileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = image.ContentType
                });
            }

            return buffered;
        }
    }
}
