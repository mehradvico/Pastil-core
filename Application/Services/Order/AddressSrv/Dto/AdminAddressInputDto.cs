using Application.Common.Dto.Input;

namespace Application.Services.Order.AddressSrv.Dto
{
    public class AdminAddressInputDto : BaseInputDto
    {
        public long? UserId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public string PostalCode { get; set; }
    }
}
