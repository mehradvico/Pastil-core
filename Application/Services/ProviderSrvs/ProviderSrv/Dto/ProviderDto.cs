using Application.Common.Dto.Field;

namespace Application.Services.ProviderSrvs.ProviderSrv.Dto
{
    public class ProviderDto : Id_FieldDto
    {
        public long UserId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string Note { get; set; }
    }
}
