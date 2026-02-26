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
    public class FloodEventController : ControllerBase
    {
        private readonly EventsDBContext _eventsContext;
        private readonly IMapper _mapper;

        public FloodEventController(EventsDBContext eventsContext, IMapper mapper)
        {
            _eventsContext = eventsContext;
            _mapper = mapper;
        }

        /// <summary>
        /// GET /api/staff/flood-events - Xem toàn bộ sự kiện ngập
        /// </summary>
        [HttpGet("flood-events")]
        public async Task<ActionResult<List<FloodEventDTO>>> GetAllFloodEvents()
        {
            try
            {
                var events = await _eventsContext.FloodEvents
                    .OrderByDescending(e => e.StartTime)
                    .ToListAsync();

                var result = _mapper.Map<List<FloodEventDTO>>(events);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}

