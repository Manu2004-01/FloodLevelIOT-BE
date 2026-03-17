using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;
using WebAPI.Helpers;

namespace WebAPI.Controllers
{
    [Route("api/staff")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class MaintenanceScheduleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MaintenanceScheduleController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("schedules")]
        public async Task<ActionResult> CreateSchedule([FromQuery] CreateMaintenanceScheduleDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu lịch bảo trì là bắt buộc"));

                var result = await _unitOfWork.ManageMaintenanceScheduleRepository.AddNewScheduleAsync(dto);

                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Tạo lịch bảo trì không thành công"));

                return Ok(new BaseCommentResponse(200, "Tạo lịch bảo trì thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpGet("schedules")]
        public async Task<ActionResult> GetAllSchedules([FromQuery] int pagenumber = 1, [FromQuery] int pazesize = 10, [FromQuery] string? status = null, [FromQuery] string? type = null, [FromQuery] string? mode = null)
        {
            try
            {
                var schedules = await _unitOfWork.ManageMaintenanceScheduleRepository.GetAllSchedulesAsync(new EntityParam
                {
                    Pagenumber = pagenumber,
                    Pagesize = pazesize,
                    ScheduleStatus = status,
                    ScheduleType = type,
                    ScheduleMode = mode
                });

                var total = await _unitOfWork.ManageMaintenanceScheduleRepository.CountAsync();

                var result = _mapper.Map<List<MaintenanceScheduleDTO>>(schedules);

                return Ok(new Pagination<MaintenanceScheduleDTO>(pazesize, pagenumber, total, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPut("schedules/{id}")]
        public async Task<ActionResult> UpdateSchedule(int id, [FromQuery] UpdateMaintenanceScheduleDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));
                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu lịch bảo trì là bắt buộc"));
                var result = await _unitOfWork.ManageMaintenanceScheduleRepository.UpdateScheduleAsync(id, dto);
                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Cập nhật lịch bảo trì không thành công"));
                if (result == false)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy lịch bảo trì"));
                return Ok(new BaseCommentResponse(200, "Cập nhật lịch bảo trì thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpDelete("schedules/{id}")]
        public async Task<ActionResult> DeleteSchedule(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));
                var result = await _unitOfWork.ManageMaintenanceScheduleRepository.DeleteScheduleAsync(id);
                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Xóa lịch bảo trì không thành công"));
                if (result == false)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy lịch bảo trì"));
                return Ok(new BaseCommentResponse(200, "Xóa lịch bảo trì thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
