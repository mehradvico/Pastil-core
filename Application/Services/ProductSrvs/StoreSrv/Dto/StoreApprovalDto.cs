using Application.Common.Dto.Field;

namespace Application.Services.ProductSrvs.StoreSrv.Dto
{
    public class StoreApprovalDto : Id_FieldDto
    {
        public bool Approved { get; set; }
        public string ApprovalValue { get; set; }
    }
}
