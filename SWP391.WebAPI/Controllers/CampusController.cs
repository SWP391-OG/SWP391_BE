using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Campus;
using SWP391.Contracts.Common;
using SWP391.Contracts.Location;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;

namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public CampusController(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }

        /// <summary>
        /// Get all campuses
        /// </summary>
        /// <response code="200">Returns all campuses.</response>
        /// <response code="404">No campuses found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CampusDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize]
        public async Task<IActionResult> GetAllCampuses()
        {
            var campuses = await _applicationServices.CampusService.GetAllCampus();

            if (campuses == null || !campuses.Any())
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No campuses found"));
            }

            return Ok(ApiResponse<List<CampusDto>>.SuccessResponse(campuses, "Campuses retrieved successfully"));
        }

        /// <summary>
        /// Get campus by code
        /// </summary>
        /// <param name="campusCode">The campus code</param>
        /// <response code="200">Returns the campus.</response>
        /// <response code="400">Invalid campus code.</response>
        /// <response code="404">Campus not found.</response>
        [HttpGet("{campusCode}")]
        [ProducesResponseType(typeof(ApiResponse<CampusDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize]
        public async Task<IActionResult> GetCampusByCode(string campusCode)
        {
            if (string.IsNullOrWhiteSpace(campusCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Campus code cannot be empty"));
            }

            var campus = await _applicationServices.CampusService.GetCampusByCode(campusCode);

            if (campus == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Campus not found"));
            }

            return Ok(ApiResponse<CampusDto>.SuccessResponse(campus, "Campus retrieved successfully"));
        }

        ///// <summary>
        ///// Get all locations by campus code
        ///// </summary>
        ///// <param name="campusCode">The campus code</param>
        ///// <response code="200">Returns all locations for the campus.</response>
        ///// <response code="400">Invalid campus code.</response>
        ///// <response code="404">Campus not found or no locations available.</response>
        //[HttpGet("{campusCode}/locations")]
        //[ProducesResponseType(typeof(ApiResponse<List<LocationDto>>), ApiStatusCode.OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        //[Authorize]
        //public async Task<IActionResult> GetLocationsByCampusCode(string campusCode)
        //{
        //    if (string.IsNullOrWhiteSpace(campusCode))
        //    {
        //        return BadRequest(ApiResponse<object>.ErrorResponse("Campus code cannot be empty"));
        //    }

        //    var locations = await _applicationServices.CampusService.GetLocationsByCampusCodeAsync(campusCode);

        //    if (locations == null || !locations.Any())
        //    {
        //        return NotFound(ApiResponse<object>.ErrorResponse("No locations found for this campus"));
        //    }

        //    return Ok(ApiResponse<List<LocationDto>>.SuccessResponse(locations, 
        //        $"Retrieved {locations.Count} locations for campus '{campusCode}'"));
        //}
    }
}
