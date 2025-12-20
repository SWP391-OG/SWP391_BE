using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Category;
using SWP391.Contracts.Common;
using SWP391.Contracts.Department;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;
using System.Security.Claims;

namespace SWP391.WebAPI.Controllers
{
    /// <summary>
    /// API controller for managing categories
    /// </summary>
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
        /// Get all categories (admin sees all, others see active only)
        /// </summary>
        /// <response code="200">Returns all categories.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        /// <response code="404">No categories found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize]
        public async Task<IActionResult> GetAllCategory()
        {

            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;           
            var categories = new List<CategoryDto>();
            if (userRoleClaim == "Admin")
            {
                categories = await _applicationServices.CategoryService.GetAllCategoryAsync();
                if (categories == null || !categories.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("No locations found"));
                }
            }
            else
            {
                categories = await _applicationServices.CategoryService.GetAllActiveCategoriesAsync();
                if (categories == null || !categories.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("No locations found"));
                }
            }

            return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories, "Categories retrieved successfully"));
        }

        /// <summary>
        /// Get category by code
        /// </summary>
        /// <param name="categoryCode">The category code</param>
        /// <response code="200">Returns the category.</response>
        /// <response code="400">Invalid category code.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="404">Category not found.</response>
        [HttpGet("{categoryCode}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize]
        public async Task<IActionResult> GetByCategoryCode(string categoryCode)
        {
            if (string.IsNullOrWhiteSpace(categoryCode))
                return BadRequest(ApiResponse<object>.ErrorResponse("Category code is required"));

            var category = await _applicationServices.CategoryService.GetByCategoryCodeAsync(categoryCode);

            if (category == null)          
                return NotFound(ApiResponse<object>.ErrorResponse($"Category with code '{categoryCode}' not found"));
            
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category retrieved successfully"));
        }

        /// <summary>
        /// Create a new category (admin only)
        /// </summary>
        /// <param name="dto">Category creation data</param>
        /// <response code="201">Category created successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can create categories.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), ApiStatusCode.CREATED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequestDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Category data is required"));

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

            return StatusCode(ApiStatusCode.CREATED, ApiResponse<CategoryDto>.SuccessResponse(data, message));
        }

         /// <summary>
        /// Update category information (admin only)
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="dto">Category update data</param>
        /// <response code="200">Category updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can update categories.</response>
        /// <response code="404">Category not found.</response>
        [HttpPut("{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] CategoryRequestDto dto)
        {
            if (categoryId <= 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid category ID"));

            if (dto == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Category data is required"));

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices
                .CategoryService.UpdateCategoryAsync(categoryId, dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Update category status (ACTIVE or INACTIVE)
        /// </summary>
        /// <param name="dto">Status update data</param>
        /// <response code="200">Category status updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can update status.</response>
        /// <response code="404">Category not found.</response>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategoryStatus(CategoryStatusUpdateDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Status update data is required"));

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices
                .CategoryService.UpdateStatusCategoryAsync(dto);

            if (!success)           
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            
            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Delete a category by code
        /// </summary>
        /// <param name="categoryId">The category code to delete</param>
        /// <response code="200">Category deleted successfully.</response>
        /// <response code="400">Invalid request or business rule violation.</response>
        /// <response code="404">Category not found.</response>
        //[HttpDelete("{categoryId}")]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> DeleteCategory(int categoryId)
        //{
        //    var (success, message) = await _applicationServices
        //        .CategoryService.DeleteCategoryAsync(categoryId);

        //    if (!success)
        //    {
        //        return BadRequest(ApiResponse<object>.ErrorResponse(message));
        //    }

        //    return Ok(ApiResponse<object>.SuccessResponse(null, message));
        //}
    }
}
