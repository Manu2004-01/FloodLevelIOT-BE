using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LocationController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("locations")]
        public async Task<ActionResult> GetAllLocations()
        {
            try
            {
                var locations = await _unitOfWork.LocationRepository.GetAllAsync();
                if (locations != null)
                {
                    var result = _mapper.Map<IReadOnlyList<Location>, IReadOnlyList<ManageLocationDTO>>(locations);
                    return Ok(result);
                }
                return NotFound(new BaseCommentResponse(404, "Không tìm thấy địa điểm"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
