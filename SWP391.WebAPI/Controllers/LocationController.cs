using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Authentication;
using SWP391.Contracts.Common;
using SWP391.Contracts.Location;
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
        /// Get all room 
        /// </summary>
        [HttpGet("/api/Locations")]
        public async Task<IActionResult> GetAllLocationCode()
        {
            var locations = await _applicationServices.LocationService.GetAllLocationsAsync();
            if (locations == null)
            {
                return NotFound();
            }
            return Ok(locations);
        }

        /// <summary>
        /// Get room by code
        /// </summary>
        [HttpGet("{locationCode}")]
        public async Task<IActionResult> GetByLocationCode(string locationCode)
        {
            var locations = await _applicationServices.LocationService.GetByLocationCodeAsync(locationCode);
            if (locations == null)
            {
                return NotFound();
            }
            return Ok(locations);
        }

        /// <summary>
        /// Create room 
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
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
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
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
        /// Update room status
        /// </summary>
        [HttpPatch("status")]
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
        [HttpDelete]
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
