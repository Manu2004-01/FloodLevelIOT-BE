using AutoMapper;
using Core.DTOs;
using Infrastructure.DBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/staff")]
    [ApiController]
    public class SensorReadingController : ControllerBase
    {
        private readonly EventsDBContext _eventsContext;
        private readonly IMapper _mapper;

        public SensorReadingController(EventsDBContext eventsContext, IMapper mapper)
        {
            _eventsContext = eventsContext;
            _mapper = mapper;
        }

        /// <summary>
        /// GET /api/staff/sensor-readings - Xem toàn bộ dữ liệu đo cảm biến
        /// </summary>
        [HttpGet("sensor-readings")]
        public async Task<ActionResult<List<SensorReadingDTO>>> GetAllSensorReadings()
        {
            try
            {
                var readings = await _eventsContext.SensorReadings
                    .OrderByDescending(r => r.RecordedAt)
                    .ToListAsync();

                var result = _mapper.Map<List<SensorReadingDTO>>(readings);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}

