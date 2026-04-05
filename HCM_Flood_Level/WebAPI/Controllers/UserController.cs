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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public UserController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("users")]
        public async Task<ActionResult> GetAllAcc([FromQuery] int pagenumber = 1, [FromQuery] int pagesize = 10, [FromQuery] string? search = null, [FromQuery] int? roleid = null)
        {
            try
            {
                if (pagenumber <= 0 || pagesize <= 0)
                    return BadRequest(new BaseCommentResponse(400, "Số trang và kích thước trang phải lớn hơn 0"));

                var acc = await _unitOfWork.ManageUserRepository.GetAllUserAsync(new EntityParam
                {
                    Pagenumber = pagenumber,
                    Pagesize = pagesize,
                    Search = search,
                    RoleId = roleid
                });

                var total = await _unitOfWork.ManageUserRepository.CountAsync(u => u.RoleId != 1);

                var result = _mapper.Map<List<UserDTO>>(acc);

                return Ok(new Pagination<UserDTO>(pagesize, pagenumber, total, result));
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

        [HttpPost("technicians")]
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

        [HttpPut("technicians/{id}")]
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

        [HttpDelete("technicians/{id}")]
        public async Task<ActionResult> DeleteAcc(int id)
        {
            try
            {
                if(id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var result = await _unitOfWork.ManageUserRepository.DeleteStaffAsync(id);

                if (result == StaffDeleteUserResult.UserNotFound)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));

                if (result == StaffDeleteUserResult.TargetNotTechnician)
                    return BadRequest(new BaseCommentResponse(400, "Chỉ được xóa tài khoản kỹ thuật viên. Không được xóa công dân."));

                if (result == StaffDeleteUserResult.TechnicianHasIncompleteWork)
                    return BadRequest(new BaseCommentResponse(400, "Không thể xóa kỹ thuật viên khi còn yêu cầu bảo trì hoặc lịch bảo trì chưa hoàn thành. Vui lòng phân công kỹ thuật viên khác trước khi xóa."));

                return Ok(new BaseCommentResponse(200, "Đã xóa người dùng thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
