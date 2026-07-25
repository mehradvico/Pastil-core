using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.CategorySrv.Dto;
using Application.Services.CategorySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// دسته بندی ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private ICategoryService categoryService;
        /// <summary>
        /// دسته بندی
        /// </summary>
        /// 
        public CategoryController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CategoryDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await categoryService.FindAsyncDto(id);
            return Ok(item);
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resultEnum"></param>
        /// <returns></returns>
        /// 
        [HttpGet("{id}/{resultEnum}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Get(long id, CategoryResultEnum resultEnum)
        {
            switch (resultEnum)
            {
                case CategoryResultEnum.Simple:
                    {
                        var item = await categoryService.FindVDtoAsync(id);
                        return Ok(item);
                    }
                case CategoryResultEnum.Children:
                    {
                        var item = await categoryService.FindChildrenVDtoAsync(id);
                        return Ok(item);

                    }
                case CategoryResultEnum.Parent:
                    {
                        var item = await categoryService.FindParentVDtoAsync(id);
                        return Ok(item);

                    }
                case CategoryResultEnum.Complete:
                    {
                        var item = await categoryService.FindCompleteVDtoAsync(id);
                        return Ok(item);
                    }
                default:

                    {
                        var item = await categoryService.FindVDtoAsync(id);
                        return Ok(item);
                    }
            }
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [ProducesResponseType(typeof(CategorySearchDto), 200)]
        public IActionResult Get([FromQuery] CategoryInputDto dto)
        {
            var searchDto = categoryService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// 
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CategoryDto>), 200)]
        public async Task<IActionResult> Post(CategoryDto categoryDto)
        {
            var insertDto = await categoryService.InsertAsyncDto(categoryDto);
            return Ok(insertDto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CategoryDto>), 200)]
        public IActionResult Put(CategoryDto categoryDto)
        {
            var updateDto = categoryService.UpdateDto(categoryDto);
            return Ok(updateDto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CategoryDto>), 200)]
        public IActionResult Delete(long id)
        {
            var deleteDto = categoryService.DeleteDto(id);
            return Ok(deleteDto);
        }
    }
}
