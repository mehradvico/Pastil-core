using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolInsightSrv;
using Application.Services.SchoolSrvs.SchoolTeachingSrv;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>اطلاعات عمومی مدرسه بدون ورود (۲۰۲۶-۱۰-۲۸): نظرها/امتیازهای دوره و استعلام گواهی.</summary>
    [Route("api/School")]
    [ApiController]
    public class SchoolPublicController : ControllerBase
    {
        private readonly ISchoolInsightService _insight;
        private readonly ISchoolTeachingService _teaching;

        public SchoolPublicController(ISchoolInsightService insight, ISchoolTeachingService teaching)
        {
            _insight = insight;
            _teaching = teaching;
        }

        /// <summary>نظرها و امتیازهای یک دوره (نظرهای ردشده نمی‌آیند)</summary>
        [HttpGet("Course/{courseId:long}/Rates")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolCourseRateVDto>>), 200)]
        public async Task<IActionResult> CourseRates(long courseId, [FromQuery] int take = 50) => Ok(await _insight.CourseRatesAsync(courseId, take));

        /// <summary>استعلام گواهی پایان دوره با کد (مثلاً PSC-AB12CD34)</summary>
        [HttpGet("Certificate/{code}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCertificateVDto>), 200)]
        public async Task<IActionResult> Certificate(string code) => Ok(await _teaching.VerifyCertificateAsync(code));
    }
}
