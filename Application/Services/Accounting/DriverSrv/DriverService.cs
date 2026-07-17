using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.DriverSrv.Dto;
using Application.Services.Accounting.DriverSrv.Iface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using DocumentFormat.OpenXml.Office.CustomUI;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services.Accounting.DriverSrv
{
    public class DriverService : CommonSrv<Driver, DriverDto>, IDriverService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly INoticeService _noticeService;
        public DriverService(IDataBaseContext _context, IMapper mapper, IUserService userService, ICurrentUserHelper currentUser, INoticeService noticeService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._userService = userService;
            this._currentUser = currentUser;
            this._noticeService = noticeService;
        }

        public async Task<BaseResultDto<DriverVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Drivers.Include(s => s.CertificatePicture).Include(s => s.ProfilePicture).Include(s => s.VehicleCardPicture).Include(s => s.City).ThenInclude(s => s.State).Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.Status).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<DriverVDto>(true, mapper.Map<DriverVDto>(item));
            }
            return new BaseResultDto<DriverVDto>(false, mapper.Map<DriverVDto>(item));
        }

        public DriverSearchDto Search(DriverInputDto baseSearchDto)
        {
            var model = _context.Drivers.Include(s => s.CertificatePicture).Include(s => s.ProfilePicture).Include(s => s.VehicleCardPicture).Include(s => s.City).ThenInclude(s => s.State).Include(s => s.Neighborhood).Include(s => s.Owner).Include(s => s.Status).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.NeighborhoodId.HasValue)
            {
                model = model.Where(s => s.Neighborhood.Id == baseSearchDto.NeighborhoodId.Value);
            }
            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s => s.City.Id == baseSearchDto.CityId.Value);
            }
            if (baseSearchDto.StatusId.HasValue)
            {
                model = model.Where(s => s.StatusId == baseSearchDto.StatusId.Value);
            }
            if (baseSearchDto.Approved.HasValue)
            {
                model = model.Where(s => s.Approved == baseSearchDto.Approved.Value);
            }
            if (baseSearchDto.OwnerId.HasValue)
            {
                model = model.Where(s => s.Owner.Id == baseSearchDto.OwnerId.Value);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q));
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Vehicle))
            {
                model = model.Where(s => s.Vehicle.Contains(baseSearchDto.Vehicle));
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new DriverSearchDto(baseSearchDto, model, mapper);
        }
        public override async Task<BaseResultDto<DriverDto>> InsertAsyncDto(DriverDto dto)
        {
            try
            {
                var modelCheker = await InsertCheckerAsync(dto);
                if (!modelCheker.IsSuccess)
                {
                    return new BaseResultDto<DriverDto>(false, dto);
                }
                else
                {
                    var item = mapper.Map<Driver>(dto);
                    if (dto.Rate != 0 && (dto.Rate > 5 || dto.Rate < 1))
                    {
                        return new BaseResultDto<DriverDto>(false, val1: Resource.Notification.TheRangeEnteredIsNotCorrect, val2: nameof(dto.Rate), data: dto);
                    }
                    var ownerId = dto.OwnerId;
                    var model = await _context.Users.Include(s => s.Driver).FirstOrDefaultAsync(s => s.Id == ownerId && !s.Deleted);
                    if (model == null)
                    {
                        return new BaseResultDto<DriverDto>(false, Resource.Notification.NothingFound, dto);
                    }
                    if (model.Driver != null)
                    {
                        return new BaseResultDto<DriverDto>(false, Resource.Notification.AlreadyIsDriver, dto);
                    }
                    await _context.Drivers.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.DriverSubmitted, ActorUserId = item.OwnerId, ReferenceType = "Driver", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.DriverSubmitted}:{item.Id}", Metadata = new Dictionary<string, string> { { "driverName", item.Name } } });
                    return new BaseResultDto<DriverDto>(true, mapper.Map<DriverDto>(item));

                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<DriverDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
        public async Task<BaseResultDto> InsertCheckerAsync(DriverDto dto)
        {
            dto.Phone = await dto.Phone?.Trim().ToEnglishDigitsAsync();
            var errors = new List<Tuple<string, string>>();

            if (string.IsNullOrEmpty(dto.Name))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheName, nameof(dto.Name)));
            }
            if (string.IsNullOrEmpty(dto.Phone))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterThePhone, nameof(dto.Phone)));
            }
            if (string.IsNullOrEmpty(dto.Vehicle))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheVehicle, nameof(dto.Vehicle)));
            }
            if (string.IsNullOrEmpty(dto.LicensePlateNumber))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheLicensePlateNumber, nameof(dto.LicensePlateNumber)));
            }
            if (errors.Any())
            {
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            return new BaseResultDto(true);
        }
        public override BaseResultDto UpdateDto(DriverDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<DriverDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<Driver>(dto);
                    var driver = _context.Drivers.FirstOrDefault(x => x.Id == item.Id);
                    _context.Drivers.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public async Task<BaseResultDto> UpdateAsyncDto(DriverDto dto)
        {
            var result = UpdateDto(dto);
            if (result.IsSuccess)
                await _noticeService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.DriverUpdated, ActorUserId = dto.OwnerId, ReferenceType = "Driver", ReferenceId = dto.Id, DeduplicationKey = $"{NoticeTypeLabels.DriverUpdated}:{dto.Id}:{DateTime.UtcNow.Ticks}", Metadata = new Dictionary<string, string> { { "driverName", dto.Name } } });
            return result;
        }

        public BaseResultDto DriverUpdateStatusDto(DriverUpdateStatusDto dto)
        {
            try
            {
                var driver = _context.Drivers.FirstOrDefault(x => x.Id == dto.Id);
                if (dto.StatusId == (long)DriverRequestStatusEnum.DriverRequestStatus_Rejected)
                {
                    if (string.IsNullOrEmpty(dto.AdminDetail))
                    {
                        new Tuple<string, string>(Resource.Notification.PleaseEnterTheAdminDetail, nameof(dto.AdminDetail));
                    }
                    driver.AdminDetail = dto.AdminDetail;
                    driver.StatusId = dto.StatusId;
                    driver.Approved = false;
                }
                else
                {
                    driver.StatusId = dto.StatusId;
                    driver.Approved = true;
                }
                _context.Drivers.Update(driver);
                _context.SaveChanges();
                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
    }
}
