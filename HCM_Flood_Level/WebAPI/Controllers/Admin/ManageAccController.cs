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
    [Route("api/[controller]")]
    [ApiController]
    public class ManageAccController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ManageAccController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("manage-acc")]
        public async Task<ActionResult> GetAllAcc([FromQuery] int pagenumber, [FromQuery] int pazesize, [FromQuery] string? search = null)
        {
            try
            {
                if (pagenumber <= 0 || pazesize <= 0)
                    return BadRequest(new BaseCommentResponse(400, "Số trang và kích thước trang phải lớn hơn 0"));

                var acc = await _unitOfWork.ManageAccRepository.GetAllAccAsync(new EntityParam
                {
                    Pagenumber = pagenumber,
                    Pagesize = pazesize,
                    Search = search
                });

                var total = await _unitOfWork.ManageAccRepository.CountAsync();

                var result = _mapper.Map<List<ManageAccDTO>>(acc);

                return Ok(new Pagination<ManageAccDTO>(pazesize, pagenumber, total, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpGet("acc/{id}")]
        public async Task<ActionResult> GetAccById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var acc = await _unitOfWork.ManageAccRepository.GetByIdAsync(id, u => u.Role);
                if (acc == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy tài khoản"));

                var result = _mapper.Map<AccDTO>(acc);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPost("create-acc")]
        public async Task<ActionResult> CreateAcc([FromQuery] CreateAccDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu không hợp lệ"));

                if(dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu người dùng là bắt buộc"));

                var result = await _unitOfWork.ManageAccRepository.AddNewAccAsync(dto);

                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Tạo tài khoản không thành công"));

                return Ok(new BaseCommentResponse(200, "Tạo tài khoản thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
