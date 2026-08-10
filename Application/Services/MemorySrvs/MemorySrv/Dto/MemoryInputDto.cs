using Application.Common.Dto.Input;
using System;

namespace Application.Services.MemorySrvs.MemorySrv.Dto
{
    public class MemoryInputDto : BaseInputDto
    {
        public long? UserId { get; set; }
        public long? UserPetId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
