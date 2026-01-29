using AutoMapper;
using Core.DTOs.Admin;
using Core.Interfaces;
using Core.Sharing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;
using WebAPI.Helpers;

namespace WebAPI.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class ManageStaffController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ManageStaffController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("staffs")]
        public async Task<ActionResult> GetAllAcc([FromQuery] int pagenumber = 1, [FromQuery] int pazesize = 10, [FromQuery] string? search = null)
        {
            try
            {
                if (pagenumber <= 0 || pazesize <= 0)
                    return BadRequest(new BaseCommentResponse(400, "Số trang và kích thước trang phải lớn hơn 0"));

                var acc = await _unitOfWork.ManageAccRepository.GetAllStaffAsync(new EntityParam
                {
                    Pagenumber = pagenumber,
                    Pagesize = pazesize,
                    Search = search
                });

                var total = await _unitOfWork.ManageAccRepository.CountAsync();

                var result = _mapper.Map<List<ManageStaffDTO>>(acc);

                return Ok(new Pagination<ManageStaffDTO>(pazesize, pagenumber, total, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpGet("staffs/{id}")]
        public async Task<ActionResult> GetAccById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var acc = await _unitOfWork.ManageAccRepository.GetByIdAsync(id, u => u.Role);
                if (acc == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy tài khoản"));

                var result = _mapper.Map<StaffDTO>(acc);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPost("staffs")]
        public async Task<ActionResult> CreateAcc([FromQuery] CreateStaffDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu không hợp lệ"));

                if(dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu người dùng là bắt buộc"));

                var result = await _unitOfWork.ManageAccRepository.AddNewStaffAsync(dto);

                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Tạo tài khoản không thành công"));

                return Ok(new BaseCommentResponse(200, "Tạo tài khoản thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPut("staffs/{id}")]
        public async Task<ActionResult> UpdateAcc(int id, [FromQuery] UpdateStaffDTO dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                if (!dto.RoleId.HasValue && !dto.Status.HasValue)
                    return BadRequest(new BaseCommentResponse(400, "Cần cung cấp ít nhất một trường để cập nhật"));
                
                var result = await _unitOfWork.ManageAccRepository.UpdateStaffAsync(id, dto);

                if (result == false)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));

                return Ok(new BaseCommentResponse(200, "Cập nhật tài khoản thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpDelete("staffs/{id}")]
        public async Task<ActionResult> DeleteAcc(int id)
        {
            try
            {
                if(id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var result = await _unitOfWork.ManageAccRepository.DeleteStaffAsync(id);

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
