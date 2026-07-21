using Application.Common.Dto.Result;
using Application.Services.LocationFields.UserCurrentLocationSrv.Dto;
using Application.Services.LocationFields.UserCurrentLocationSrv.Iface;
using AutoMapper;
using Entities.Entities.LocationField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.UserCurrentLocationSrv.Dto
{
    public class UserCurrentLocationSearchDto : BaseSearchDto<UserCurrentLocation, UserCurrentLocationVDto>, IUserCurrentLocationSearchFields
    {
        public UserCurrentLocationSearchDto(UserCurrentLocationInputDto dto, IQueryable<UserCurrentLocation> list, IMapper mapper) : base(dto, list, mapper)
        {
            UserId = dto.UserId;
            CityId = dto.CityId;
            NeighborhoodId = dto.NeighborhoodId;
        }
        public long? UserId { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
    }
}
