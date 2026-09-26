using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceDriverSrv.Dto;
using Application.Services.FinanceSrvs.FinanceDriverSrv.Iface;
using Application.Common.Enumerable.Code;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceDriverSrv
{
    /// <summary>
    /// حسابداری رانندگان پت‌رسان: لیست با آمار سفرها و سهم‌ها، جزئیات یک راننده و تنظیم درصد کمیسیون سایت.
    /// </summary>
    public class FinanceDriverService : IFinanceDriverService
    {
        private readonly IDataBaseContext _context;

        public FinanceDriverService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<FinanceDriverListDto> SearchAsync(FinanceDriverInputDto dto)
        {
            dto ??= new FinanceDriverInputDto();
            var pageIndex = Math.Max(1, dto.PageIndex);
            var pageSize = Math.Clamp(dto.PageSize, 1, 200);

            var drivers = _context.Drivers.AsNoTracking().Where(d => !d.Deleted);

            if (dto.Available.HasValue)
                drivers = drivers.Where(d => d.Active == dto.Available.Value);
            if (dto.HasCommission.HasValue)
                drivers = dto.HasCommission.Value
                    ? drivers.Where(d => d.CommissionPercent > 0)
                    : drivers.Where(d => d.CommissionPercent == null || d.CommissionPercent == 0);
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var q = dto.Q.Trim();
                drivers = drivers.Where(d => d.Name.Contains(q) || d.Phone.Contains(q) ||
                                             d.LicensePlateNumber.Contains(q) || d.Owner.Mobile.Contains(q));
            }

            drivers = dto.SortBy == 2 ? drivers.OrderBy(d => d.Id) : drivers.OrderByDescending(d => d.Id);

            var totalCount = await drivers.CountAsync();
            var page = await drivers.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(d => new FinanceDriverVDto
            {
                Id = d.Id,
                Name = d.Name,
                Phone = d.Phone,
                OwnerMobile = d.Owner != null ? d.Owner.Mobile : null,
                Vehicle = d.Vehicle,
                LicensePlateNumber = d.LicensePlateNumber,
                Active = d.Active,
                Approved = d.Approved,
                CommissionPercent = d.CommissionPercent
            }).ToListAsync();

            await FillTripStatsAsync(page);
            return new FinanceDriverListDto { List = page, TotalCount = totalCount, PageIndex = pageIndex, PageSize = pageSize };
        }

        public async Task<BaseResultDto<FinanceDriverDetailVDto>> DetailAsync(long driverId)
        {
            var driver = await _context.Drivers.AsNoTracking()
                .Where(d => d.Id == driverId && !d.Deleted)
                .Select(d => new FinanceDriverDetailVDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Phone = d.Phone,
                    OwnerMobile = d.Owner != null ? d.Owner.Mobile : null,
                    Vehicle = d.Vehicle,
                    LicensePlateNumber = d.LicensePlateNumber,
                    Active = d.Active,
                    Approved = d.Approved,
                    CommissionPercent = d.CommissionPercent
                })
                .FirstOrDefaultAsync();

            if (driver == null)
                return new BaseResultDto<FinanceDriverDetailVDto>(false, Resource.Notification.NothingFound, null);

            var asList = new System.Collections.Generic.List<FinanceDriverVDto> { driver };
            await FillTripStatsAsync(asList);

            driver.Trips = await _context.Trips.AsNoTracking()
                .Where(t => t.DriverId == driverId)
                .OrderByDescending(t => t.Id)
                .Take(200)
                .Select(t => new FinanceDriverTripVDto
                {
                    TripId = t.Id,
                    CreateDate = t.CreateDate,
                    TripStatusId = t.TripStatusId,
                    Price = t.Price,
                    PaymentPrice = t.PaymentPrice,
                    DriverShare = t.DriverShare,
                    SiteShare = t.SiteShare,
                    IsPaid = t.IsPaid
                })
                .ToListAsync();

            return new BaseResultDto<FinanceDriverDetailVDto>(true, driver);
        }

        public async Task<BaseResultDto> UpdateCommissionAsync(FinanceDriverCommissionDto dto)
        {
            if (dto == null || dto.CommissionPercent < 0 || dto.CommissionPercent > 100)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var driver = await _context.Drivers.AsTracking().FirstOrDefaultAsync(d => d.Id == dto.Id && !d.Deleted);
            if (driver == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            driver.CommissionPercent = Math.Round(dto.CommissionPercent, 2);
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // آمار سفرهای یک صفحه از رانندگان با یک کوئری گروهی (نه N+1)
        private async Task FillTripStatsAsync(System.Collections.Generic.List<FinanceDriverVDto> drivers)
        {
            if (drivers.Count == 0) return;
            var ids = drivers.Select(d => d.Id).ToList();
            var completed = (long)TripStatusEnum.TripStatus_Compeleted;
            var canceled = (long)TripStatusEnum.TripStatus_Canceled;

            var stats = await _context.Trips.AsNoTracking()
                .Where(t => t.DriverId.HasValue && ids.Contains(t.DriverId.Value))
                .GroupBy(t => t.DriverId.Value)
                .Select(g => new
                {
                    DriverId = g.Key,
                    Trips = g.Count(),
                    Completed = g.Count(t => t.TripStatusId == completed),
                    Canceled = g.Count(t => t.TripStatusId == canceled),
                    Payment = g.Where(t => t.TripStatusId == completed).Sum(t => (double?)t.PaymentPrice) ?? 0,
                    DriverShare = g.Where(t => t.TripStatusId == completed).Sum(t => (double?)t.DriverShare) ?? 0,
                    SiteShare = g.Where(t => t.TripStatusId == completed).Sum(t => (double?)t.SiteShare) ?? 0
                })
                .ToListAsync();

            foreach (var d in drivers)
            {
                var s = stats.FirstOrDefault(x => x.DriverId == d.Id);
                if (s == null) continue;
                d.TripCount = s.Trips;
                d.CompletedTripCount = s.Completed;
                d.CanceledTripCount = s.Canceled;
                d.TotalPayment = s.Payment;
                d.TotalDriverShare = s.DriverShare;
                d.TotalSiteShare = s.SiteShare;
            }
        }
    }
}
