using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Application.Common.Service
{
    public abstract class CommonSrv<TEntity, TDto> : ICommonSrv<TEntity, TDto> where TEntity : class
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private DbSet<TEntity> entity;
        public CommonSrv(IDataBaseContext _context, IMapper mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            entity = _context.Set<TEntity>();

        }

        public virtual async Task<BaseResultDto<TDto>> FindAsyncDto(long id)
        {
            var item = await entity.FindAsync(id);
            if (item != null)
                return new BaseResultDto<TDto>(true, mapper.Map<TDto>(item));
            return new BaseResultDto<TDto>(false, mapper.Map<TDto>(item));
        }
        public virtual async Task<BaseResultDto<TDto>> FirstOrDefaultAsyncDto(Expression<Func<TEntity, bool>> predicate)
        {
            var item = await entity.Where<TEntity>(predicate).FirstOrDefaultAsync();
            return new BaseResultDto<TDto>(true, mapper.Map<TDto>(item));

        }
        public virtual async Task<BaseResultDto<TDto>> FirstOrDefaultAsyncDto()
        {
            var item = await entity.FirstOrDefaultAsync();
            return new BaseResultDto<TDto>(true, mapper.Map<TDto>(item));
        }
        public virtual async Task<BaseResultDto<TDto>> InsertAsyncDto(TDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<TDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<TEntity>(dto);
                    PropertyInfo pi = item.GetType().GetProperty("CreateDate");
                    if (pi != null)
                    {
                        pi.SetValue(item, DateTime.Now);
                    }
                    PropertyInfo up = item.GetType().GetProperty("UpdateDate");
                    if (pi != null)
                    {
                        pi.SetValue(item, DateTime.Now);
                    }
                    await entity.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<TDto>(true, mapper.Map<TDto>(item));
                }

            }
            catch (Exception)
            {
                return new BaseResultDto<TDto>(isSuccess: false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }
        public virtual BaseResultDto UpdateDto(TDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<TDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<TEntity>(dto);
                    PropertyInfo pi = item.GetType().GetProperty("UpdateDate");
                    if (pi != null)
                    {
                        pi.SetValue(item, DateTime.Now);
                    }
                    entity.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
        }
        public virtual BaseResultDto UpdateRangeDto(List<TDto> dtos)
        {
            try
            {
                var model = mapper.Map<List<TEntity>>(dtos);
                entity.UpdateRange(model);
                _context.SaveChanges();
                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
        }
        public virtual BaseResultDto DeleteDto(long id)
        {
            try
            {
                var item = entity.Find(id);
                var navigationProperties = _context.Entry(item).Navigations;

                foreach (var navigation in navigationProperties)
                {
                    if (navigation.CurrentValue is IEnumerable<object> relatedCollection)
                    {
                        // اگر رابطه مجموعه‌ای باشد
                        foreach (var relatedItem in relatedCollection)
                        {
                            _context.Entry(relatedItem).State = EntityState.Detached;
                        }
                    }
                    else if (navigation.CurrentValue != null)
                    {
                        // اگر رابطه یک آبجکت تکی باشد
                        navigation.CurrentValue = null;
                    }
                }

                PropertyInfo pi = item.GetType().GetProperty("Deleted");
                if (pi != null)
                {
                    pi.SetValue(item, true);
                    entity.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    //entity.Update(item);
                }
                else
                {
                    entity.Remove(item);
                }
                _context.SaveChanges();
                return new BaseResultDto(true);
            }
            catch (Exception)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
        }
        public virtual BaseResultDto DeleteDto(TDto dto)
        {
            try
            {
                var item = mapper.Map<TEntity>(dto);
                PropertyInfo pi = item.GetType().GetProperty("Deleted");
                if (pi != null)
                {
                    pi.SetValue(item, true);
                    entity.Update(item);
                }
                else
                {
                    entity.Remove(item);
                }
                _context.SaveChanges();
                return new BaseResultDto(true);
            }
            catch (Exception)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }
        }



    }
}
