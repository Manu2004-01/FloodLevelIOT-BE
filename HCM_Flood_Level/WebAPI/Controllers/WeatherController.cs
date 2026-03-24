using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly IOpenWeatherService _openWeatherService;

        public WeatherController(IOpenWeatherService openWeatherService)
        {
            _openWeatherService = openWeatherService;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent([FromQuery] double lat, [FromQuery] double lon, CancellationToken cancellationToken)
        {
            if (lat is < -90 or > 90 || lon is < -180 or > 180)
                return BadRequest(new BaseCommentResponse(400, "Tọa độ không hợp lệ (lat: -90..90, lon: -180..180)."));

            var data = await _openWeatherService.GetCurrentByCoordinatesAsync(lat, lon, cancellationToken);
            if (data == null)
                return StatusCode(502, new BaseCommentResponse(502, "Không lấy được dữ liệu thời tiết. Kiểm tra API key OpenWeather hoặc thử lại sau."));

            return Ok(data);
        }
    }
}
