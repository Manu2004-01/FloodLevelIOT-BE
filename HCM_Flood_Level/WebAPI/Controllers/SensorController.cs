using AutoMapper;
using Core.DTOs.Sensor;
using Core.Interfaces;
using Core.Sharing;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Errors;
using WebAPI.Helpers;

namespace WebAPI.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SensorController(IUnitOfWork unitOfWork, IMapper mapper)
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

                var sensors = await _unitOfWork.SensorRepository.GetAllSensorsAsync(new EntityParam
                {
                    Pagenumber = pagenumber,
                    Pagesize = pagesize,
                    Search = search
                });

                var total = await _unitOfWork.SensorRepository.CountAsync(search);

                var result = _mapper.Map<List<SensorDTO>>(sensors);

                return Ok(new Pagination<SensorDTO>(pagesize, pagenumber, total, result));
            }
            catch (Exception ex)
            {
                var detail = ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : string.Empty);
                return StatusCode(500, new BaseCommentResponse(500, detail));
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

                var sensor = await _unitOfWork.SensorRepository.GetSensorByIdAsync(id);

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
        public async Task<ActionResult> CreateDevice([FromBody] CreateSensorDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu không hợp lệ"));

                // Validation
                if (string.IsNullOrWhiteSpace(dto.SensorCode))
                    return BadRequest(new BaseCommentResponse(400, "Mã thiết bị là bắt buộc"));

                if (string.IsNullOrWhiteSpace(dto.SensorName))
                    return BadRequest(new BaseCommentResponse(400, "Tên thiết bị là bắt buộc"));

                if (string.IsNullOrWhiteSpace(dto.SensorType))
                    return BadRequest(new BaseCommentResponse(400, "Loại thiết bị là bắt buộc"));

                if (dto.LocationId <= 0)
                    return BadRequest(new BaseCommentResponse(400, "Vị trí lắp đặt là bắt buộc"));

                // Check if sensor code already exists
                if (await _unitOfWork.SensorRepository.SensorCodeExistsAsync(dto.SensorCode))
                    return BadRequest(new BaseCommentResponse(400, $"Mã thiết bị '{dto.SensorCode}' đã tồn tại trong hệ thống"));

                var result = await _unitOfWork.SensorRepository.AddNewSensorAsync(dto);

                if (!result)
                    return BadRequest(new BaseCommentResponse(400, "Tạo thiết bị không thành công"));

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
        public async Task<ActionResult> UpdateDevice(int id, [FromBody] UpdateSensorDTO dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID thiết bị không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                // Check if at least one field is provided for update
                if (string.IsNullOrEmpty(dto.SensorName) &&
                    string.IsNullOrEmpty(dto.SensorType) &&
                    string.IsNullOrEmpty(dto.SensorStatus) &&
                    !dto.LocationId.HasValue)
                    return BadRequest(new BaseCommentResponse(400, "Cần cung cấp ít nhất một trường để cập nhật"));

                var result = await _unitOfWork.SensorRepository.UpdateSensorAsync(id, dto);

                if (!result)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy thiết bị"));

                return Ok(new BaseCommentResponse(200, "Cập nhật thiết bị thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        /// <summary>
        /// PUT /api/admin/devices/{id}/location - Cập nhật vị trí thiết bị
        /// </summary>
        [HttpPut("devices/{id}/location")]
        public async Task<ActionResult> UpdateDeviceLocation(int id, [FromBody] UpdateLocationDTO dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID thiết bị không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu không hợp lệ"));

                // Validation cho tọa độ
                if (dto.Latitude < -90 || dto.Latitude > 90)
                    return BadRequest(new BaseCommentResponse(400, "Vĩ độ (Latitude) phải trong khoảng -90 đến 90"));

                if (dto.Longitude < -180 || dto.Longitude > 180)
                    return BadRequest(new BaseCommentResponse(400, "Kinh độ (Longitude) phải trong khoảng -180 đến 180"));

                if (string.IsNullOrWhiteSpace(dto.Address))
                    return BadRequest(new BaseCommentResponse(400, "Địa chỉ là bắt buộc"));

                var result = await _unitOfWork.SensorRepository.UpdateLocationAsync(id, dto);

                if (!result)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy thiết bị hoặc vị trí"));

                return Ok(new BaseCommentResponse(200, "Cập nhật vị trí thiết bị thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        /// <summary>
        /// PUT /api/admin/devices/{id}/threshold - Cập nhật ngưỡng cảnh báo
        /// </summary>
        [HttpPut("devices/{id}/threshold")]
        public async Task<ActionResult> UpdateDeviceThreshold(int id, [FromBody] UpdateThresholdDTO dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID thiết bị không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu không hợp lệ"));

                // Validation cho ngưỡng
                //if (dto.MinThreshold.HasValue && dto.MaxThreshold.HasValue && dto.MinThreshold >= dto.MaxThreshold)
                //    return BadRequest(new BaseCommentResponse(400, "Ngưỡng tối thiểu phải nhỏ hơn ngưỡng tối đa"));

                var result = await _unitOfWork.SensorRepository.UpdateThresholdAsync(id, dto);

                if (!result)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy thiết bị"));

                return Ok(new BaseCommentResponse(200, "Cập nhật ngưỡng cảnh báo thành công"));
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

                var result = await _unitOfWork.SensorRepository.DeleteSensorAsync(id);

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