using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/citizen")]
    [ApiController]
    [Authorize(Roles = "Citizen")]
    public class CitizenController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CitizenController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPut("profile/{id}")]
        public async Task<ActionResult> UpdateProfile(int id, [FromQuery] UpdateProfileDTO dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));
                var result = await _unitOfWork.ManageUserRepository.UpdateProfileAsync(id, dto);
                if (!result)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));
                return Ok(new BaseCommentResponse(200, "Cập nhật thông tin cá nhân thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
