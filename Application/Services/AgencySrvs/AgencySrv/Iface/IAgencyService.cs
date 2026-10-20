using Application.Services.AgencySrvs.AgencySrv.Dto;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.AgencySrvs.AgencySrv.Iface
{
    /// <summary>لیست یکپارچه‌ی نمایندگی‌ها (همه‌ی انواع زیر یک مفهوم)؛ فقط خواندن. ساخت/ویرایش هر نوع در سرویس خودش می‌ماند.</summary>
    public interface IAgencyService
    {
        Task<AgencySearchVDto> SearchAsync(AgencyInputDto input);
    }
}
