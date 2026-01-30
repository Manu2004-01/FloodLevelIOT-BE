using AutoMapper;
using Core.DTOs.Sensor;
using Core.Interfaces;
using Core.Sharing;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;
using WebAPI.Helpers;

namespace WebAPI.Controllers.Sensors
{
    [Route("api/admin")]
    [ApiController]
    public class ManageSensorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ManageSensorController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// GET /api/admin/devices - Lấy danh sách thiết bị
        /// </summary>
        [HttpGet("devices")]
        public async Task<ActionResult> GetAllDevices(
            [FromQuery] int pagenumber = 1,
            [FromQuery] int pagesize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                if (pagenumber <= 0 || pagesize <= 0)
                    return BadRequest(new BaseCommentResponse(400, "Số trang và kích thước trang phải lớn hơn 0"));

                var sensors = await _unitOfWork.ManageSensorRepository.GetAllSensorsAsync(new EntityParam
                {
                    Pagenumber = pagenumber,
                    Pagesize = pagesize,
                    Search = search
                });

                var total = await _unitOfWork.ManageSensorRepository.CountAsync();

                var result = _mapper.Map<List<ManageSensorDTO>>(sensors);

                return Ok(new Pagination<ManageSensorDTO>(pagesize, pagenumber, total, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        /// <summary>
        /// GET /api/admin/devices/{id} - Xem chi tiết thiết bị
        /// </summary>
        [HttpGet("devices/{id}")]
        public async Task<ActionResult> GetDeviceById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID thiết bị không hợp lệ"));

                var sensor = await _unitOfWork.ManageSensorRepository.GetByIdAsync(id,
                    s => s.Location,
                    s => s.Location.Area,
                    s => s.InstalledByStaff);

                if (sensor == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy thiết bị"));

                var result = _mapper.Map<SensorDTO>(sensor);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        /// <summary>
        /// POST /api/admin/devices - Thêm mới thiết bị
        /// </summary>
        [HttpPost("devices")]
        public async Task<ActionResult> CreateDevice([FromQuery] CreateSensorDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu thiết bị là bắt buộc"));

                // Validation
                if (string.IsNullOrWhiteSpace(dto.SensorCode))
                    return BadRequest(new BaseCommentResponse(400, "Mã thiết bị là bắt buộc"));

                if (string.IsNullOrWhiteSpace(dto.SensorName))
                    return BadRequest(new BaseCommentResponse(400, "Tên thiết bị là bắt buộc"));

                if (string.IsNullOrWhiteSpace(dto.SensorType))
                    return BadRequest(new BaseCommentResponse(400, "Loại thiết bị là bắt buộc"));

                if (dto.LocationId <= 0)
                    return BadRequest(new BaseCommentResponse(400, "Vị trí lắp đặt là bắt buộc"));

                var result = await _unitOfWork.ManageSensorRepository.AddNewSensorAsync(dto);

                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Tạo thiết bị không thành công. Vui lòng kiểm tra lại mã thiết bị, vị trí hoặc tọa độ."));

                return Ok(new BaseCommentResponse(200, "Tạo thiết bị thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        /// <summary>
        /// PUT /api/admin/devices/{id} - Cập nhật thông tin thiết bị
        /// </summary>
        [HttpPut("devices/{id}")]
        public async Task<ActionResult> UpdateDevice(int id, [FromQuery] UpdateSensorDTO dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID thiết bị không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                // Check if at least one updatable field is provided
                if (!dto.LocationId.HasValue &&
                    !dto.InstalledBy.HasValue &&
                    string.IsNullOrEmpty(dto.Specification) &&
                    string.IsNullOrEmpty(dto.SensorCode) &&
                    string.IsNullOrEmpty(dto.SensorName) &&
                    string.IsNullOrEmpty(dto.Protocol) &&
                    string.IsNullOrEmpty(dto.SensorType) &&
                    !dto.MinThreshold.HasValue &&
                    !dto.MaxThreshold.HasValue &&
                    !dto.MaxLevel.HasValue)
                {
                    return BadRequest(new BaseCommentResponse(400, "Cần cung cấp ít nhất một trường để cập nhật"));
                }

                // Verify sensor exists first to return 404 when appropriate
                var existing = await _unitOfWork.ManageSensorRepository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy thiết bị"));

                var result = await _unitOfWork.ManageSensorRepository.UpdateSensorAsync(id, dto);

                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Cập nhật thiết bị không thành công. Vui lòng kiểm tra vị trí, người lắp hoặc mã thiết bị."));

                return Ok(new BaseCommentResponse(200, "Cập nhật thiết bị thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        /// <summary>
        /// DELETE /api/admin/devices/{id} - Gỡ thiết bị khỏi hệ thống
        /// </summary>
        [HttpDelete("devices/{id}")]
        public async Task<ActionResult> DeleteDevice(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID thiết bị không hợp lệ"));

                var result = await _unitOfWork.ManageSensorRepository.DeleteSensorAsync(id);

                if (!result)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy thiết bị"));

                return Ok(new BaseCommentResponse(200, "Đã gỡ thiết bị khỏi hệ thống thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}