using AutoMapper;
using Core.DTOs;
using Infrastructure.DBContext;
using Microsoft.AspNetCore.Authorization;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SensorReadingController : ControllerBase
    {
        private readonly EventsDBContext _eventsContext;
        private readonly IMapper _mapper;
        private readonly IHistoryService _historyService;

        public SensorReadingController(EventsDBContext eventsContext, IMapper mapper, IHistoryService historyService)
        {
            _eventsContext = eventsContext;
            _mapper = mapper;
            _historyService = historyService;
        }

        [HttpGet("sensor-readings")]
        public async Task<ActionResult<List<SensorReadingDTO>>> GetAllSensorReadings()
        {
            try
            {
                var readings = await _eventsContext.SensorReadings
                    .OrderByDescending(r => r.RecordedAt)
                    .ToListAsync();

                var result = _mapper.Map<List<SensorReadingDTO>>(readings);

                // Process each reading through the HistoryService
                foreach (var reading in readings)
                {
                    await _historyService.ProcessSensorReading(reading);
                }

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}

