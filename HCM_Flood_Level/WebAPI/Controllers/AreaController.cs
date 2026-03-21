using Core.DTOs;
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

        public AreaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Lấy tất cả sensor theo mọi khu vực + đọc gần nhất.</summary>
        [HttpGet("areas")]
        public async Task<ActionResult<IReadOnlyList<AreaDTO>>> GetAll(CancellationToken cancellationToken = default)
        {
            try
            {
                var list = await _unitOfWork.AreaRepository.GetAreaSensorReadingsAsync(null, cancellationToken);
                if (list.Count == 0)
                    return NotFound(new BaseCommentResponse(404, "Không có dữ liệu khu vực / sensor"));

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
