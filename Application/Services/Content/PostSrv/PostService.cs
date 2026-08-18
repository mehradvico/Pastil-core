using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CategorySrv.Iface;
using Application.Services.Content.PostCategorySrv.Iface;
using Application.Services.Content.PostFileSrv.Iface;
using Application.Services.Content.PostHashtagSrv.Iface;
using Application.Services.Content.PostPictureSrv.Iface;
using Application.Services.Content.PostSrv.Dto;
using Application.Services.Content.PostSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.PostSrv
{
    public class PostService : CommonSrv<Post, PostDto>, IPostService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IPostHashtagService postHashtagService;
        private readonly IPostCategoryService postCategoryService;
        private readonly ICategoryService categoryService;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IPostFileService _postFileService;
        private readonly IPostPictureService _postPictureService;
        private readonly string connectionString;

        public PostService(IDataBaseContext _context, IConfiguration config, IPostPictureService postPictureService, ICategoryService categoryService, IPostHashtagService postHashtagService, IPostCategoryService postCategoryService, IMapper mapper, ICurrentUserHelper currentUserHelper, IPostFileService postFileService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this.postHashtagService = postHashtagService;
            this.postCategoryService = postCategoryService;
            this.categoryService = categoryService;
            this._postFileService = postFileService;
            this._postPictureService = postPictureService;
            this._currentUserHelper = currentUserHelper;
            this.connectionString = config.GetValue<string>(
               "connection");
        }
        public override async Task<BaseResultDto<PostDto>> FindAsyncDto(long id)
        {
            var item = await _context.Posts
                .Include(s => s.User)
                .Include(s => s.PostFiles)
                .Include(s => s.PostPictures)
                    .ThenInclude(s => s.Picture)
                .Include(s => s.Categories)
                .Include(s => s.Hashtags)
                .Include(s => s.Picture)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<PostDto>(true, mapper.Map<PostDto>(item));
            return new BaseResultDto<PostDto>(false, mapper.Map<PostDto>(item));
        }
        private IQueryable<Post> BaseSaerch(PostInputDto searchDto)
        {
            var query = _context.Posts.Include(s => s.Category).Include(s => s.User).Include(s => s.Admin).Include(s => s.Categories).Include(s => s.Hashtags).Include(s => s.Picture).Where(s => s.ParentId == null && s.Active && s.Deleted == false).IgnoreQueryFilters().AsQueryable();
            DateTime now = DateTime.Now;

            if (searchDto.Available == true)
            {
                query = query.Where(s => s.Active && s.PublishDate < now && s.AdminConfirm == true);
            }
            else
            {
                query = query.Include(s => s.Children).AsQueryable();

                if (searchDto.Edited.HasValue)
                {
                    query = query.Where(s => s.Edited == searchDto.Edited.Value);
                }
                if (searchDto.Active.HasValue)
                {
                    query = query.Where(s => s.Active == searchDto.Active.Value);
                }
                if (searchDto.AllAdminConfirm == false)
                {
                    if (searchDto.AdminConfirm.HasValue)
                    {
                        query = query.Where(s => s.AdminConfirm == searchDto.AdminConfirm.Value);
                    }
                    else
                    {
                        query = query.Where(s => s.AdminConfirm == null);
                    }
                }

                if (searchDto.Publish != null)
                {
                    now = DateTime.Now;
                    if (searchDto.Publish.Value)
                    {
                        query = query.Where(s => s.PublishDate < now);
                    }
                    else
                    {
                        query = query.Where(s => s.PublishDate > now);
                    }
                }
            }
            if (searchDto.CategoryIds != null && searchDto.CategoryIds.Any())
            {
                if (searchDto.IsAndCategories)
                    foreach (var categoryId in searchDto.CategoryIds)
                    {
                        query = query.Where(s => s.Categories.Any(a => a.Id == categoryId));
                    }
                else
                    query = query.Where(s => s.Categories.Any(x => searchDto.CategoryIds.Contains(x.Id)));


            }
            else if (searchDto.CategoryLabels != null && searchDto.CategoryLabels.Any())
            {
                if (searchDto.IsAndCategories)
                    query = query.Where(s => searchDto.CategoryLabels.All(a => s.Categories.Select(c => c.Label).Contains(a)));
                else
                    query = query.Where(s => s.Categories.Any(x => searchDto.CategoryLabels.Contains(x.Label)));

            }

            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                query = query.Where(s => s.Name.Contains(searchDto.Q));
            }
            if (searchDto.NotId.HasValue)
            {
                query = query.Where(s => s.Id != searchDto.NotId);
            }

            if (!string.IsNullOrEmpty(searchDto.Hashtags))
            {
                var list = searchDto.Hashtags.Split("-");
                foreach (var item in list)
                    query = query.Where(s => s.Hashtags.Any(s => s.Name.Equals(item)));
            }

            switch (searchDto.SortBy)
            {

                case Common.Enumerable.SortEnum.Default:
                    {
                        query = query.OrderByDescending(s => s.Id);

                        break;
                    }
                case Common.Enumerable.SortEnum.New:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        query = query.OrderBy(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Name:
                    {
                        query = query.OrderByDescending(s => s.Name);
                        break;
                    }
                case Common.Enumerable.SortEnum.MoreVisit:
                    {
                        query = query.OrderByDescending(s => s.VisitCount);

                        break;
                    }
                case Common.Enumerable.SortEnum.LessVisit:
                    {
                        query = query.OrderBy(s => s.VisitCount);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessPriority:
                    {
                        query = query.OrderBy(s => s.PublishDate);
                        break;
                    }
                case Common.Enumerable.SortEnum.MorePriority:
                    {
                        query = query.OrderByDescending(s => s.PublishDate);
                        break;
                    }
                default:
                    break;
            }

            return query;
        }
        public PostSearchDto Search(PostInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto);
            return new PostSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<PostDto>> InsertAsyncDto(PostDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<PostDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var pictureValidation = ValidatePictures(dto);
                    if (!pictureValidation.IsSuccess)
                    {
                        return new BaseResultDto<PostDto>(false, pictureValidation.Messages, dto);
                    }

                    var item = mapper.Map<Post>(dto);
                    item.ParentId = item.ParentId == 0 ? null : item.ParentId;
                    item.CreateDate = DateTime.Now;
                    if (item.PublishDate < DateTime.Now)
                    {
                        item.PublishDate = DateTime.Now;
                    }
                    item.UserId = _currentUserHelper.CurrentUser.UserId;
                    await _context.Posts.AddAsync(item);
                    await _context.SaveChangesAsync();
                    if (dto.CategoryIds == null)
                    {
                        dto.CategoryIds = new List<long>();
                    }
                    if (dto.CategoryId.HasValue)
                    {
                        var categoryParents = await categoryService.GetAllParents(dto.CategoryId.Value);
                        foreach (var categoryParent in categoryParents)
                        {
                            dto.CategoryIds.Add(categoryParent.Id);

                        }
                    }
                    if (dto.CategoryIds.Any())
                    {
                        dto.CategoryIds = dto.CategoryIds.Distinct().ToList();
                        postCategoryService.InsertOrUpdate(item, dto.CategoryIds);
                    }
                    if (dto.HashTagList != null && dto.HashTagList.Any())
                    {
                        dto.HashTagList = dto.HashTagList.Distinct().ToList();

                        postHashtagService.InsertOrUpdate(item, dto.HashTagList);

                    }
                    if (dto.PostFilesList != null && dto.PostFilesList.Any())
                    {
                        _postFileService.InsertOrUpdate(item, dto.PostFilesList);
                    }
                    if (dto.PostPicturesList != null && dto.PostPicturesList.Any())
                    {
                        _postPictureService.InsertOrUpdate(item, dto.PostPicturesList);
                    }
                    return new BaseResultDto<PostDto>(true, mapper.Map<PostDto>(item));

                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<PostDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public override BaseResultDto UpdateDto(PostDto dto)
        {
            try
            {
                var orginal = _context.Posts.AsTracking().FirstOrDefault(p => p.Id == dto.Id);
                if (orginal == null)
                {
                    return new BaseResultDto(isSuccess: false, val: "پست موردنظر پیدا نشد.");
                }

                if (_currentUserHelper.CurrentUser.RoleEnum == RoleEnum.Operator.ToString())
                {
                    if (orginal.UserId != _currentUserHelper.CurrentUser.UserId)
                    {
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied);
                    }
                }

                // دسترسی به Endpoint توسط Permission کنترل می‌شود؛ ویرایش همیشه روی همان پست انجام می‌شود.
                // فیلد Edited دیگر نباید باعث مسدود شدن ویرایش یا ساخت نسخه فرزند شود.
                return UpdatingDto(dto);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        private BaseResultDto UpdatingDto(PostDto dto)
        {
            using var transaction = _context.BeginTransaction();

            try
            {
                var modelCheker = ModelHelper<PostDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var pictureValidation = ValidatePictures(dto);
                    if (!pictureValidation.IsSuccess)
                    {
                        return pictureValidation;
                    }

                    var item = _context.Posts
                        .Include(s => s.Hashtags)
                        .Include(s => s.PostFiles)
                        .Include(s => s.PostPictures)
                        .Include(s => s.Categories)
                        .AsTracking()
                        .FirstOrDefault(s => s.Id == dto.Id);

                    if (item == null)
                    {
                        return new BaseResultDto(isSuccess: false, val: "پست موردنظر پیدا نشد.");
                    }

                    var userId = item.UserId;
                    var parentId = item.ParentId;
                    var createDate = item.CreateDate;
                    var visitCount = item.VisitCount;
                    var commentCount = item.CommentCount;
                    var edited = item.Edited;
                    var deleted = item.Deleted;

                    mapper.Map(dto, item);

                    item.UserId = userId;
                    item.ParentId = parentId;
                    item.CreateDate = createDate;
                    item.VisitCount = visitCount;
                    item.CommentCount = commentCount;
                    item.Edited = edited;
                    item.Deleted = deleted;
                    item.UpdateDate = DateTime.Now;

                    _context.Posts.Update(item);
                    _context.SaveChanges();
                    if (dto.CategoryIds == null)
                    {
                        dto.CategoryIds = new List<long>();
                    }
                    if (dto.CategoryId.HasValue)
                    {
                        var categoryParents = categoryService.GetAllParents(dto.CategoryId.Value).Result;
                        foreach (var categoryParent in categoryParents)
                        {
                            dto.CategoryIds.Add(categoryParent.Id);
                        }
                    }
                    dto.CategoryIds = dto.CategoryIds.Distinct().ToList();
                    postCategoryService.InsertOrUpdate(item, dto.CategoryIds);

                    if (dto.HashTagList != null)
                    {
                        dto.HashTagList = dto.HashTagList.Distinct().ToList();
                        postHashtagService.InsertOrUpdate(item, dto.HashTagList);
                    }


                    _postFileService.InsertOrUpdate(item, dto.PostFilesList);
                    _postPictureService.InsertOrUpdate(item, dto.PostPicturesList);

                    transaction.Commit();
                    return new BaseResultDto<PostDto>(true, mapper.Map<PostDto>(item));
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        private BaseResultDto ValidatePictures(PostDto dto)
        {
            if (dto.PictureId.HasValue && !_context.Pictures.Any(s => s.Id == dto.PictureId.Value))
            {
                return new BaseResultDto(isSuccess: false, val: "تصویر اصلی پست معتبر نیست یا حذف شده است.");
            }

            if (dto.PostPicturesList == null)
            {
                return new BaseResultDto(true);
            }

            dto.PostPicturesList = dto.PostPicturesList
                .Where(s => s != null)
                .GroupBy(s => s.PictureId)
                .Select(s => s.First())
                .ToList();

            var pictureIds = dto.PostPicturesList.Select(s => s.PictureId).ToList();
            var validPictureCount = _context.Pictures.Count(s => pictureIds.Contains(s.Id));
            if (validPictureCount != pictureIds.Count)
            {
                return new BaseResultDto(isSuccess: false, val: "یک یا چند تصویر ضمیمه پست معتبر نیست یا حذف شده است.");
            }

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto<PostVDto>> FindAsyncVDto(long id, bool visit = true)
        {
            var item = await _context.Posts.Include(s => s.User).Include(s => s.Picture).Include(s => s.PostFiles).Include(s => s.PostPictures).ThenInclude(s => s.Picture).Include(s => s.Hashtags).Include(s => s.Category).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).FirstOrDefaultAsync(s => s.Id == id && s.Active && s.Deleted != true && s.AdminConfirm == true && s.PublishDate < DateTime.Now);
            if (item != null)
            {
                if (visit)
                {
                    item.VisitCount++;
                    _context.Posts.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                return new BaseResultDto<PostVDto>(true, mapper.Map<PostVDto>(item));
            }
            return new BaseResultDto<PostVDto>(false, mapper.Map<PostVDto>(item));
        }
        public async Task<BaseResultDto<PostVDto>> GetByUrlAsyncVDto(string url, bool visit = true)
        {
            var item = await _context.Posts.Include(s => s.User).Include(s => s.Picture).Include(s => s.PostFiles).Include(s => s.PostPictures).ThenInclude(s => s.Picture).Include(s => s.Hashtags).Include(s => s.Category).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).FirstOrDefaultAsync(s => s.SeoH1.Contains(url) && s.Active && s.Deleted != true);
            if (item != null)
            {
                if (visit)
                {
                    item.VisitCount++;
                    _context.Posts.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                return new BaseResultDto<PostVDto>(true, mapper.Map<PostVDto>(item));
            }
            return new BaseResultDto<PostVDto>(false, mapper.Map<PostVDto>(item));
        }

        public BaseResultDto Confirm(PostConfirmDto dto)
        {

            try
            {
                var modelCheker = ModelHelper<PostConfirmDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = _context.Posts.AsTracking().FirstOrDefault(s => s.Id == dto.Id);
                    if (dto.AdminConfirm)
                    {
                        if (item.ParentId != null)
                        {
                            var itemDto = FindAsyncDto(item.Id).Result;
                            itemDto.Data.Id = item.ParentId.Value;
                            UpdatingDto(itemDto.Data);
                            item.Deleted = true;
                            _context.Posts.Update(item);
                            _context.SaveChanges();
                            var itemEdited = _context.Posts.AsTracking().FirstOrDefault(s => s.Id == itemDto.Data.Id);
                            itemEdited.Edited = false;
                            itemEdited.ParentId = null;
                            itemEdited.AdminConfirm = dto.AdminConfirm;
                            itemEdited.AdminId = _currentUserHelper.CurrentUser.UserId;
                            itemEdited.PublishDate = dto.PublishDate;
                            _context.Posts.Update(itemEdited);
                            _context.SaveChanges();
                        }
                    }

                    item.AdminConfirm = dto.AdminConfirm;
                    item.AdminId = _currentUserHelper.CurrentUser.UserId;
                    item.PublishDate = dto.PublishDate;
                    _context.Posts.Update(item);
                    _context.SaveChanges();

                    return new BaseResultDto(true);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public override BaseResultDto DeleteDto(long id)
        {
            try
            {
                var item = _context.Posts.Include(s => s.Parent).Include(s => s.Children).FirstOrDefault(s => s.Id == id);
                if (_currentUserHelper.CurrentUser.RoleEnum == RoleEnum.Operator.ToString())
                {
                    if (item.UserId != _currentUserHelper.CurrentUser.UserId)
                    {
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied);
                    }
                }
                item.Deleted = true;
                if (item.Parent != null)
                {
                    item.Parent.Edited = false;
                }
                _context.Posts.Update(item);
                foreach (var p in item.Children)
                {
                    p.Deleted = true;
                    _context.Posts.Update(p);
                }
                _context.SaveChanges();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public void UpdatePostCommentCount(long postId)
        {
            var item = _context.Posts.Include(s => s.PostComments).ThenInclude(s => s.Status).AsTracking().FirstOrDefault(s => s.Id == postId);
            item.CommentCount = item.PostComments.Count(c => c.Status.Label == CommentEnum.Comment_Accept.ToString());
            _context.Posts.Update(item);
            _context.SaveChanges();
        }
        public BaseResultDto ChangeUser(ChangePostUserDto dto)
        {

            try
            {
                var item = _context.Posts.AsTracking().FirstOrDefault(s => s.Id == dto.Id);
                item.UserId = dto.UserId;
                _context.Posts.Update(item);
                _context.SaveChanges();
                return new BaseResultDto(true);

            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        public BaseResultDto GetSiteMap()
        {
            string sqlQuery = $"SELECT dbo.Posts.Id, dbo.Posts.Name,dbo.Posts.UpdateDate, dbo.Categories.Label As CategoryName FROM dbo.Posts INNER JOIN dbo.Categories ON dbo.Posts.CategoryId = dbo.Categories.Id WHERE dbo.Posts.Active = 1 and dbo.Posts.Deleted=0";
            //var list = _context.Posts.Include(s => s.Category).Where(s => s.Deleted == false && s.Active && s.AdminConfirm == true).Select(s => new PostSiteMapDto() { Id = s.Id, Name = s.Name, CategoryName = s.Category.Label,UpdateDate=s.PublishDate }).ToList();
            var connection = new SqlConnection(connectionString);
            var posts = connection.Query<PostSiteMapDto>(sqlQuery).ToList();
            return new BaseResultDto<List<PostSiteMapDto>>(true, posts);
        }
    }

}
