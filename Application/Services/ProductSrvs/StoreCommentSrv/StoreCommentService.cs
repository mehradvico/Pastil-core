using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.ProductSrvs.StoreCommentSrv.Dto;
using Application.Services.ProductSrvs.StoreCommentSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.StoreSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.StoreCommentSrv
{
    public class StoreCommentService : CommonSrv<StoreComment, StoreCommentDto>, IStoreCommentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService codeService;
        private readonly IStoreService _storeService;
        public StoreCommentService(IDataBaseContext _context, IMapper mapper, ICodeService codeService, IStoreService storeService) : base(_context, mapper)
        {
            this.codeService = codeService;
            this._context = _context;
            this.mapper = mapper;
            this._storeService = storeService;
        }

        public override async Task<BaseResultDto<StoreCommentDto>> InsertAsyncDto(StoreCommentDto dto)
        {
            try
            {
                dto.Text = await SanitizeTextHelper.ToSanitizeAsync(dto.Text);
                var modelCheker = ModelHelper<StoreCommentDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (string.IsNullOrEmpty(dto.Name))
                    {
                        return new BaseResultDto<StoreCommentDto>(false, val1: Resource.Notification.PleaseEnterTheName, val2: nameof(dto.Name), data: dto);
                    }

                    if (dto.Rate.HasValue && (dto.Rate.Value > 5 || dto.Rate.Value < 1))
                    {
                        return new BaseResultDto<StoreCommentDto>(false, val1: Resource.Notification.TheRangeEnteredIsNotCorrect, val2: nameof(dto.Rate), data: dto);
                    }
                    var item = mapper.Map<StoreComment>(dto);
                    var commentStatus = await codeService.GetByLabelAsync(CommentEnum.Comment_NotChecked.ToString());
                    if (commentStatus != null)
                    {
                        item.StatusId = commentStatus.Id;
                        item.CreateDate = DateTime.Now;
                        item.Answer = null;
                        await _context.StoreComments.AddAsync(item);
                        _context.SaveChanges();
                        return new BaseResultDto<StoreCommentDto>(true, mapper.Map<StoreCommentDto>(item));
                    }
                    return new BaseResultDto<StoreCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
                }
            }
            catch
            {
                return new BaseResultDto<StoreCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }
        public override BaseResultDto UpdateDto(StoreCommentDto dto)
        {
            try
            {
                var item = _context.StoreComments.FirstOrDefault(s => s.Id.Equals(dto.Id));
                if (item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);
                if (item.StoreId != dto.StoreId)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied);
                item.StatusId = dto.StatusId;
                item.Answer = dto.Answer;
                _context.StoreComments.Update(item);
                _context.SaveChanges();
                _storeService.UpdateStoreCommentRateAsync(item.StoreId);
                return new BaseResultDto(isSuccess: true);

            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public StoreCommentSearchDto Search(StoreCommentInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto);
            return new StoreCommentSearchDto(baseSearchDto, model, mapper);
        }
        private IQueryable<StoreComment> BaseSaerch(StoreCommentInputDto searchDto)
        {
            var query = _context.StoreComments.Include(s => s.Store).AsQueryable();
            if (searchDto.StoreId.HasValue)
                query = query.Where(s => s.StoreId == searchDto.StoreId).Include(s => s.User);
            if (searchDto.AllStatus == false)
            {

                switch (searchDto.SortBy)
                {

                    case Common.Enumerable.SortEnum.Default:
                        {
                            query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.New:
                        {
                            query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Old:
                        {
                            query = query.OrderBy(s => s.StatusId).ThenBy(s => s.Id);
                            break;
                        }

                    default:
                        query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);

                        break;
                }
            }


            return query;
        }

    }
}
