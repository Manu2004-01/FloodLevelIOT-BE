using AutoMapper;
using Core.DTOs.Area;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;

namespace WebAPI.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AreaController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("areas")]
        public async Task<ActionResult> GetAllAreas()
        {
            try
            {
                var areas = await _unitOfWork.AreaRepository.GetAllAsync();
                if (areas != null)
                {
                    var result = _mapper.Map<IReadOnlyList<Area>, IReadOnlyList<ManageAreaDTO>>(areas);
                    return Ok(result);
                }
                return NotFound(new BaseCommentResponse(404, "Không tìm thấy khu vực"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
