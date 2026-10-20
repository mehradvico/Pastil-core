using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProviderSrvs.SubscriptionPlanSrv.Dto;
using Entities.Entities.ProviderField;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.SubscriptionPlanSrv.Iface
{
    public interface ISubscriptionPlanService : ICommonSrv<SubscriptionPlan, SubscriptionPlanDto>
    {
        SubscriptionPlanSearchDto Search(SubscriptionPlanInputDto input);
    }
}
