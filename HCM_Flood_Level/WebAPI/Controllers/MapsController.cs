using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MapsController : ControllerBase
    {
        private readonly IMapsService _mapsService;
        private readonly IRouteAvoidFloodService _routeAvoidFloodService;

        public MapsController(IMapsService mapsService, IRouteAvoidFloodService routeAvoidFloodService)
        {
            _mapsService = mapsService;
            _routeAvoidFloodService = routeAvoidFloodService;
        }

        [HttpGet("search-map")]
        public async Task<IActionResult> Search([FromQuery] MapsSearchDTO dto)
        {
            try
            {

                var result = await _mapsService.SearchAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi khi tìm kiếm bản đồ: " + ex.Message));
            }
        }

        [Authorize(Roles = "Citizen")]
        [HttpPost("route-avoid-flood")]
        public async Task<ActionResult<RouteAvoidFloodResponseDTO>> RouteAvoidFlood([FromQuery] RouteAvoidFloodRequestDTO request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu request là bắt buộc"));

                var response = await _routeAvoidFloodService.GetAvoidFloodRouteAsync(request, HttpContext.RequestAborted);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var details = ex.ToString();
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi khi gợi ý đường đi tránh ngập lụt: " + details));
            }
        }
    }
}
