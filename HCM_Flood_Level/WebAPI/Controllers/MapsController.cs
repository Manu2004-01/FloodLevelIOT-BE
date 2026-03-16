
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/maps")]
    [ApiController]
    [Authorize]
    public class MapsController : ControllerBase
    {
        private readonly IMapsService _mapsService;

        public MapsController(IMapsService mapsService)
        {
            _mapsService = mapsService;
        }

        [HttpGet("search")]
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
    }
}
