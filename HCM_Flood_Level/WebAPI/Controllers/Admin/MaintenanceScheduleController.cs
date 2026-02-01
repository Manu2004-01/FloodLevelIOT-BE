using AutoMapper;
using Core.DTOs.Admin;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;

namespace WebAPI.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
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
    }
}
