using Application.Common.Enumerable;
using Application.Services.AgencySrvs.AgencySrv.Dto;
using Application.Services.AgencySrvs.AgencySrv.Iface;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.AgencySrvs.AgencySrv
{
    public class AgencyService : IAgencyService
    {
        private readonly IDataBaseContext _context;

        public AgencyService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<AgencySearchVDto> SearchAsync(AgencyInputDto input)
        {
            var model = AgencyQueries.Combined(_context, input);
            var total = await model.CountAsync();

            var ordered = AgencyQueries.Order(model, input.SortBy, DateTime.Now);

            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Clamp(input.PageSize, 1, 200);
            var rows = await ordered.Skip((pageIndex - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();
            var list = rows.Select(r => r.ToView()).ToList();

            return new AgencySearchVDto { PageIndex = pageIndex, PageSize = pageSize, TotalCount = total, List = list };
        }
    }
}
