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
    public class HistoryController : ControllerBase
    {
        private readonly EventsDBContext _eventsContext;
        private readonly IMapper _mapper;

        public HistoryController(EventsDBContext eventsContext, IMapper mapper)
        {
            _eventsContext = eventsContext;
            _mapper = mapper;
        }

        /// <summary>
        /// GET /api/staff/flood-events - Xem toàn bộ sự kiện ngập
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<List<HistoryDTO>>> GetAllHistories()
        {
            try
            {
                var histories = await _eventsContext.Histories
                    .OrderByDescending(e => e.StartTime)
                    .ToListAsync();

                var result = _mapper.Map<List<HistoryDTO>>(histories);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}

