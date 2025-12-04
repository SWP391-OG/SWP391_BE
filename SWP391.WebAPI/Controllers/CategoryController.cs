using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts;
using SWP391.Contracts.Authentication;
using SWP391.Contracts.Common;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;

namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public CategoryController(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }

        /// <summary>
        /// Get all category 
        /// </summary>
        [HttpGet("/api/Category")]
        public async Task<IActionResult> GetAllCategory()
        {
            var categories = await _applicationServices.CategoryService.GetAllCategoryAsync();
            if (categories == null)
            {
                return NotFound();
            }
            return Ok(categories);
        }

        /// <summary>
        /// Get category by code
        /// </summary>
        [HttpGet("{categoryCode}")]
        public async Task<IActionResult> GetByCategoryCode(string categoryCode)
        {
            var category = await _applicationServices.CategoryService.GetByCategoryCodeAsync(categoryCode);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        /// <summary>
        /// Create category 
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> CreateCategory(CategoryRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message, data) = await _applicationServices
                 .CategoryService.CreateCategoryAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(data, message));
        }


        /// <summary>
        /// Update category 
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryRequestDto dto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var category = await _applicationServices.CategoryService.UpdateCategoryAsync(dto);

            var (success, message) = await _applicationServices
               .CategoryService.UpdateCategoryAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(null, message));

        }

        /// <summary>
        /// Update category status
        /// </summary>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> UpdateCategoryStatus([FromBody] CategoryStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }
            var (success, message) = await _applicationServices
                .CategoryService.UpdateStatusCategoryAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Delete category status
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> DeleteCategoryByCode([FromQuery] string categoryCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }
            var (success, message) = await _applicationServices
                .CategoryService.DeleteCategoryByCodeAsync(categoryCode);
            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(null, message));
        }
    }
}
