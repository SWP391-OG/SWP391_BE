using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts;
using SWP391.Contracts.Authentication;
using SWP391.Contracts.Common;
using SWP391.Contracts.Location;
using SWP391.Contracts.Ticket;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;


namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public LocationController(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }

        /// <summary>
        /// Get all rooms 
        /// </summary>
        /// <param >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated location.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpGet("/api/Locations")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize]
        public async Task<IActionResult> GetAllLocationCode()
        {
            var locations = await _applicationServices.LocationService.GetAllLocationsAsync();
            if (locations == null || !locations.Any())
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No categories found"));
            }
            return Ok(ApiResponse<List<LocationDto>>.SuccessResponse(locations, "Categories retrieved successfully"));
        }

        /// <summary>
        /// Get room by code
        /// </summary>
        /// <param name="locationCode">Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated location.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpGet("{locationCode}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize]
        public async Task<IActionResult> GetByLocationCode(string locationCode)
        {
            var locations = await _applicationServices.LocationService.GetByLocationCodeAsync(locationCode);
            if (locations == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No categories found"));
            }
            return Ok(ApiResponse<LocationDto>.SuccessResponse(locations, "Categories retrieved successfully"));
        }

        /// <summary>
        /// Create room 
        /// </summary>
        /// <response code="200">Returns paginated location.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLocation(LocationRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message, data) = await _applicationServices
                 .LocationService.CreateLocationAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<LocationDto>.SuccessResponse(data, message));
        }


        /// <summary>
        /// Update room 
        /// </summary>
        ///  /// <response code="200">Returns paginated location .</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationRequestDto dto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var locations = await _applicationServices.LocationService.UpdateLocationAsync(dto);

            var (success, message) = await _applicationServices
               .LocationService.UpdateLocationAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<LocationDto>.SuccessResponse(null, message));

        }

        /// <summary>
        /// Update Location Status (ACTIVE or INACTIVE)
        /// </summary>
        /// <response code="200">Returns paginated location.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLocationStatus([FromBody] LocationStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }
            var (success, message) = await _applicationServices
                .LocationService.UpdateStatusLocationAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<LocationDto>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Update room status
        /// </summary> 
        /// <response code="200">Returns paginated location.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLocationByCode([FromQuery] string locationCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }
            var (success, message) = await _applicationServices
                .LocationService.DeleteLocationByCodeAsync(locationCode);
            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }
            return Ok(ApiResponse<LocationDto>.SuccessResponse(null, message));
        }


    }
}
