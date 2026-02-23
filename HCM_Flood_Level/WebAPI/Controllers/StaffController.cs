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
    public class StaffController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public StaffController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("users")]
        public async Task<ActionResult> GetAllAcc([FromQuery] int pagenumber = 1, [FromQuery] int pazesize = 10, [FromQuery] string? search = null, [FromQuery] int? roleid = null)
        {
            try
            {
                if (pagenumber <= 0 || pazesize <= 0)
                    return BadRequest(new BaseCommentResponse(400, "Số trang và kích thước trang phải lớn hơn 0"));

                var acc = await _unitOfWork.ManageUserRepository.GetAllStaffAsync(new EntityParam
                {
                    Pagenumber = pagenumber,
                    Pagesize = pazesize,
                    Search = search,
                    RoleId = roleid
                });

                var total = await _unitOfWork.ManageUserRepository.CountAsync();

                var result = _mapper.Map<List<ManageUserDTO>>(acc);

                return Ok(new Pagination<ManageUserDTO>(pazesize, pagenumber, total, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult> GetAccById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var acc = await _unitOfWork.ManageUserRepository.GetByIdAsync(id, u => u.Role);
                if (acc == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy tài khoản"));

                var result = _mapper.Map<UserDTO>(acc);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPost("users")]
        public async Task<ActionResult> CreateAcc([FromQuery] CreateUserDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu người dùng là bắt buộc"));

                var result = await _unitOfWork.ManageUserRepository.AddNewStaffAsync(dto);

                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Tạo tài khoản không thành công"));

                return Ok(new BaseCommentResponse(200, "Tạo tài khoản thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult> UpdateAcc(int id, [FromQuery] UpdateUserDTO dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                if (!dto.RoleId.HasValue && !dto.Status.HasValue)
                    return BadRequest(new BaseCommentResponse(400, "Cần cung cấp ít nhất một trường để cập nhật"));
                
                var result = await _unitOfWork.ManageUserRepository.UpdateStaffAsync(id, dto);

                if (result == false)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));

                return Ok(new BaseCommentResponse(200, "Cập nhật tài khoản thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteAcc(int id)
        {
            try
            {
                if(id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var result = await _unitOfWork.ManageUserRepository.DeleteStaffAsync(id);

                if (result == false)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));

                return Ok(new BaseCommentResponse(200, "Đã xóa người dùng thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
