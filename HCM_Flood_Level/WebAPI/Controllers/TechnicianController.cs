using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Errors;
using WebAPI.Helpers;

namespace WebAPI.Controllers
{
    [Route("api/technician")]
    [ApiController]
    [Authorize(Roles = "Technician")]
    public class TechnicianController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TechnicianController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("schedules")]
        public async Task<ActionResult> GetMySchedules([FromQuery] int pagenumber = 1, [FromQuery] int pazesize = 10, [FromQuery] string? status = null, [FromQuery] string? type = null)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                   ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var technicianId))
                {
                    return Unauthorized(new BaseCommentResponse(401, "Không xác định được danh tính kỹ thuật viên"));
                }

                var schedules = await _unitOfWork.ManageMaintenanceScheduleRepository.GetSchedulesByTechnicianAsync(technicianId, new EntityParam
                {
                    Pagenumber = pagenumber,
                    Pagesize = pazesize,
                    ScheduleStatus = status,
                    ScheduleType = type
                });

                var total = await _unitOfWork.ManageMaintenanceScheduleRepository.CountAsync(s => s.AssignedTechnicianId == technicianId);

                var result = _mapper.Map<List<MaintenanceScheduleDTO>>(schedules);

                return Ok(new Pagination<MaintenanceScheduleDTO>(pazesize, pagenumber, total, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ: " + ex.Message));
            }
        }

        [HttpPut("schedules/{id}/status")]
        public async Task<ActionResult> UpdateMyScheduleStatus(int id, [FromQuery] string status)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                   ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var technicianId))
                {
                    return Unauthorized(new BaseCommentResponse(401, "Không xác định được danh tính kỹ thuật viên"));
                }

                // First check if the schedule exists and belongs to the current technician
                var schedule = await _unitOfWork.ManageMaintenanceScheduleRepository.GetByIdAsync(id);
                if (schedule == null || schedule.AssignedTechnicianId != technicianId)
                {
                    return NotFound(new BaseCommentResponse(404, "Lịch bảo trì không tồn tại hoặc không được giao cho bạn"));
                }

                var result = await _unitOfWork.ManageMaintenanceScheduleRepository.UpdateScheduleStatusAsync(id, status);

                if (!result)
                {
                    return BadRequest(new BaseCommentResponse(400, "Cập nhật trạng thái không thành công (trạng thái không hợp lệ)"));
                }

                return Ok(new BaseCommentResponse(200, "Cập nhật trạng thái thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ: " + ex.Message));
            }
        }
    }
}
