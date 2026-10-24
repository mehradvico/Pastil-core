using Application.Common.Dto.Result;
using Application.Services.Accounting.OtpVerifySrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.Accounting.OtpVerifySrv.Iface
{
    public interface IOtpVerifyService
    {
        Task<BaseResultDto<OtpVerifyVDto>> InsertAsyncDto(OtpVerifyVDto dto);
        Task<BaseResultDto> CheckVerify(OtpVerifyVDto dto);
        Task<BaseResultDto> IsVerified(OtpVerifyVDto dto);
        /// <summary>کد تأییدشده را باطل می‌کند تا همان کد دوباره (تا ۵ دقیقه) برای کار دیگری قابل استفاده نباشد.</summary>
        Task ConsumeAsync(string mobile, string email);
    }
}
