using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Services;
using Infrastructure.DBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Errors;

namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ManageDBContext _context;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ManageDBContext context, ITokenService tokenService, IUnitOfWork unitOfWork, IConfiguration configuration, INotificationService notificationService, ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromQuery] LoginDTO login)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (login == null)
                    return BadRequest(new BaseCommentResponse(400, "Thiếu thông tin đăng nhập"));

                if (string.IsNullOrWhiteSpace(login.Email))
                    return BadRequest(new BaseCommentResponse(400, "Email là bắt buộc"));

                if (string.IsNullOrWhiteSpace(login.Password))
                    return BadRequest(new BaseCommentResponse(400, "Mật khẩu là bắt buộc"));

                var log = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == login.Email);

                if (log == null)
                    return Unauthorized(new BaseCommentResponse(401, "Tên đăng nhập hoặc mật khẩu không đúng"));

                if(!PasswordHelper.VerifyPassword(login.Password, log.PasswordHash))
                    return Unauthorized(new BaseCommentResponse(401, "Mật khẩu không đúng"));
                
                var roleName = log.Role?.RoleName ?? string.Empty;
                var token = _tokenService.CreateToken(log, roleName);

                return Ok(new {token});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                return Ok(new BaseCommentResponse(200, "Đăng xuất thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromQuery] RegisterDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Thiếu thông tin đăng ký"));

                var hasPhone = !string.IsNullOrWhiteSpace(dto.PhoneNumber);
                var hasEmail = !string.IsNullOrWhiteSpace(dto.Email);

                if (!hasPhone && !hasEmail)
                    return BadRequest(new BaseCommentResponse(400, "Phải cung cấp số điện thoại hoặc email"));

                if (hasPhone && hasEmail)
                    return BadRequest(new BaseCommentResponse(400, "Chỉ nên đăng ký bằng số điện thoại HOẶC email"));

                if (hasEmail)
                {
                    var existedEmail = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == dto.Email);
                    if (existedEmail != null)
                        return BadRequest(new BaseCommentResponse(400, "Email đã tồn tại"));
                }

                if (hasPhone)
                {
                    var existedPhone = await _context.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);
                    if (existedPhone != null)
                        return BadRequest(new BaseCommentResponse(400, "Số điện thoại đã tồn tại"));
                }

                var defaultRoleId = 3;

                var user = new User
                {
                    FullName = dto.FullName ?? string.Empty,
                    Email = hasEmail ? dto.Email! : null,
                    PhoneNumber = hasPhone ? dto.PhoneNumber! : null,
                    PasswordHash = PasswordHelper.HashPassword(dto.Password),
                    RoleId = defaultRoleId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                try
                {
                    if (hasPhone)
                    {
                        var otp = GenerateOtpCode();
                        var message = $"Mã OTP đăng ký tài khoản của bạn là: {otp}. Vui lòng không chia sẻ mã này cho bất kỳ ai.";
                        await _notificationService.SendSmsAsync(NormalizePhoneNumber(dto.PhoneNumber!), message);
                    }
                    else if (hasEmail)
                    {
                        var subject = "Đăng ký tài khoản thành công";
                        var body = $"Xin chào {user.FullName},\n\nTài khoản của bạn đã được tạo thành công.\nBạn có thể đăng nhập bằng email này.\n\nTrân trọng.";
                        await _notificationService.SendEmailAsync(dto.Email!, subject, body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gửi SMS/Email thất bại sau khi đăng ký. UserId hoặc SĐT/Email có thể cần kiểm tra.");
                }

                return Ok(new BaseCommentResponse(200, "Đăng ký tài khoản thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            var normalized = phoneNumber.Trim();
            if (normalized.StartsWith("0"))
            {
                var withoutLeadingZero = normalized.TrimStart('0');
                // Di động VN: 9x, 8x, 7x, 5x, 3x (9 chữ số)
                if (withoutLeadingZero.Length == 9 && "35789".Contains(withoutLeadingZero[0]))
                    return "+84" + withoutLeadingZero;
                return "+84" + withoutLeadingZero; // vẫn chuẩn hóa, có thể log warning nếu không thuộc 35789
            }
            return normalized;
        }

        private static string GenerateOtpCode()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }
    }
}
