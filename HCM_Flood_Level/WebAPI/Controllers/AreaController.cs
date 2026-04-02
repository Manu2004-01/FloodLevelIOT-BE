using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AreaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AreaController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>Lấy tất cả sensor theo mọi khu vực + đọc gần nhất.</summary>
        [HttpGet("areas")]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var list = await _unitOfWork.AreaRepository.GetAllAsync();
                if (list == null)  
                
                    return NotFound(new BaseCommentResponse(404, "Không có dữ liệu khu vực"));

                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpGet("areas/{areaId}")]
        public async Task<ActionResult<IReadOnlyList<AreaDTO>>> GetDetail(int areaId, CancellationToken cancellationToken = default)
        {
            try
            {
                var list = await _unitOfWork.AreaRepository.GetAreaSensorReadingsAsync(areaId, cancellationToken);
                if (list.Count == 0)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy sensor trong khu vực này"));

                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
