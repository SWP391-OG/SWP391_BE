using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Common;
using SWP391.Contracts.Location;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;
using System.Security.Claims;


namespace SWP391.WebAPI.Controllers
{
    /// <summary>
    /// API controller for managing locations
    /// </summary>
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
        /// Get all locations (admin sees all, others see active only)
        /// </summary>
        /// <response code="200">Returns all locations.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        /// <response code="404">No locations found.</response>
        [HttpGet("/api/Locations")]
        [ProducesResponseType(typeof(ApiResponse<LocationDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize]
        public async Task<IActionResult> GetAllLocationCode()
        {
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var locations = new List<LocationDto>();
            if (userRoleClaim == "Admin")
            {
                locations = await _applicationServices.LocationService.GetAllLocationsAsync();
                if (locations == null || !locations.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("No locations found"));
                }
            }
            else
            {
                locations = await _applicationServices.LocationService.GetAllActiveLocationsAsync();
                if (locations == null || !locations.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("No locations found"));
                }
            }

            return Ok(ApiResponse<List<LocationDto>>.SuccessResponse(locations, "Locations retrieved successfully"));
        }

         /// <summary>
        /// Get location by code
        /// </summary>
        /// <param name="locationCode">The location code</param>
        /// <response code="200">Returns the location.</response>
        /// <response code="400">Invalid location code.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("{locationCode}")]
        [ProducesResponseType(typeof(ApiResponse<LocationDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
         [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize]
        public async Task<IActionResult> GetByLocationCode(string locationCode)
        {
            if (string.IsNullOrWhiteSpace(locationCode))
                return BadRequest(ApiResponse<object>.ErrorResponse("Location code is required"));

            var locations = await _applicationServices.LocationService.GetByLocationCodeAsync(locationCode);
            if (locations == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No location found"));
            }
            return Ok(ApiResponse<LocationDto>.SuccessResponse(locations, "Location retrieved successfully"));
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
             if (dto == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Location data is required"));

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
        /// Update location information (admin only)
        /// </summary>
        /// <param name="locationId">Location ID</param>
        /// <param name="dto">Location update data</param>
        /// <response code="200">Location updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can update locations.</response>
        /// <response code="404">Location not found.</response>
        [HttpPut("{locationId}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLocation(int locationId, [FromBody] LocationRequestDto dto)
        {
            if (locationId <= 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid location ID"));

            if (dto == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Location data is required"));

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices
               .LocationService.UpdateLocationAsync(locationId, dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<LocationDto>.SuccessResponse(null, message));

        }

         /// <summary>
        /// Update location status (ACTIVE or INACTIVE)
        /// </summary>
        /// <param name="dto">Status update data</param>
        /// <response code="200">Location status updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can update status.</response>
        /// <response code="404">Location not found.</response>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLocationStatus(LocationStatusUpdateDto dto)
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
        /// Delete room 
        /// </summary> 
        /// <response code="200">Returns paginated location.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        //[HttpDelete("{locationId}")]
        //[ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> DeleteLocation(int locationId)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ApiResponse<object>.ErrorResponse(
        //            ApiMessages.INVALID_REQUEST_DATA,
        //            ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        //    }
        //    var (success, message) = await _applicationServices
        //        .LocationService.DeleteLocationByIdAsync(locationId);
        //    if (!success)
        //    {
        //        return BadRequest(ApiResponse<object>.ErrorResponse(message));
        //    }
        //    return Ok(ApiResponse<LocationDto>.SuccessResponse(null, message));
        //}

        /// <summary>
        /// Get room by code
        /// </summary>
        /// <param name="campusCode">Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated location.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpGet("get-by/{campusCode}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<LocationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize]
        public async Task<IActionResult> GetByCampusCode(string campusCode)
        {
            var locations = await _applicationServices.LocationService.GetLocationsByCampusCodeAsync(campusCode);
            if (locations == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No location found"));
            }
            return Ok(ApiResponse<List<LocationDto>>.SuccessResponse(locations, "Location retrieved successfully"));
        }



    }
}
