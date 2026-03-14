using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Services;
using Infrastructure.DBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

        public AuthController(ManageDBContext context, ITokenService tokenService, IUnitOfWork unitOfWork, IConfiguration configuration, INotificationService notificationService)
        {
            _context = context;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _notificationService = notificationService;
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

                if (!log.IsActive)
                    return Unauthorized(new BaseCommentResponse(401, "Tài khoản chưa xác nhận OTP"));
                
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

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new BaseCommentResponse(400, "Email là bắt buộc"));

                if (string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new BaseCommentResponse(400, "Mật khẩu là bắt buộc"));

                var existedEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existedEmail != null)
                    return BadRequest(new BaseCommentResponse(400, "Email đã tồn tại"));

                var defaultRoleId = 3;
                var otp = OtpHelper.GenerateOtp6();
                var otpHash = OtpHelper.HashOtpSha256(otp);

                var user = new User
                {
                    FullName = dto.FullName ?? string.Empty,
                    Email = dto.Email.Trim(),
                    PhoneNumber = null,
                    PasswordHash = PasswordHelper.HashPassword(dto.Password),
                    RoleId = defaultRoleId,
                    IsActive = false,
                    EmailOtpHash = otpHash,
                    EmailOtpExpiredAt = DateTime.UtcNow.AddMinutes(10),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                try
                {
                    var subject = "Mã OTP xác nhận đăng ký";
                    var body = $@"Xin chào {user.FullName},

                                Mã OTP của bạn là: {otp}
                                Mã có hiệu lực trong 10 phút.

                                Trân trọng.";
                    await _notificationService.SendEmailAsync(user.Email!, subject, body);
                }
                catch (Exception)
                {
                    // Email sending failed
                    return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi khi lấy mã OTP"));
                }

                return Ok(new BaseCommentResponse(200, "Đăng ký thành công. Vui lòng kiểm tra email để lấy OTP và xác nhận tài khoản."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPost("verify-email-otp")]
        public async Task<IActionResult> VerifyEmailOtp([FromQuery] VerifyEmailOtpDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Thiếu thông tin xác nhận OTP"));

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new BaseCommentResponse(400, "Email là bắt buộc"));

                if (string.IsNullOrWhiteSpace(dto.Otp))
                    return BadRequest(new BaseCommentResponse(400, "OTP là bắt buộc"));

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));

                if (user.IsActive)
                    return Ok(new BaseCommentResponse(200, "Tài khoản đã được xác nhận trước đó"));

                if (user.EmailOtpExpiredAt == null || user.EmailOtpExpiredAt < DateTime.UtcNow)
                    return BadRequest(new BaseCommentResponse(400, "OTP đã hết hạn"));

                var inputHash = OtpHelper.HashOtpSha256(dto.Otp.Trim());
                if (!string.Equals(inputHash, user.EmailOtpHash, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new BaseCommentResponse(400, "OTP không đúng"));

                user.IsActive = true;
                user.EmailOtpHash = null;
                user.EmailOtpExpiredAt = null;

                await _context.SaveChangesAsync();

                return Ok(new BaseCommentResponse(200, "Xác nhận OTP thành công. Tài khoản đã được kích hoạt."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] ForgotPasswordDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng với email này"));

                var otp = OtpHelper.GenerateOtp6();
                var otpHash = OtpHelper.HashOtpSha256(otp);

                user.EmailOtpHash = otpHash;
                user.EmailOtpExpiredAt = DateTime.UtcNow.AddMinutes(10);

                await _context.SaveChangesAsync();

                try
                {
                    var subject = "Mã OTP đặt lại mật khẩu";
                    var body =
$@"Xin chào {user.FullName},

Mã OTP để đặt lại mật khẩu của bạn là: {otp}
Mã có hiệu lực trong 10 phút.

Trân trọng.";
                    await _notificationService.SendEmailAsync(user.Email!, subject, body);
                }
                catch (Exception)
                {
                    // Email sending failed
                    return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi khi lấy mã OTP"));
                }

                return Ok(new BaseCommentResponse(200, "Yêu cầu đặt lại mật khẩu thành công. Vui lòng kiểm tra email để lấy OTP."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng với email này"));

                if (user.EmailOtpHash == null || user.EmailOtpExpiredAt < DateTime.UtcNow)
                    return BadRequest(new BaseCommentResponse(400, "OTP không hợp lệ hoặc đã hết hạn"));

                var inputHash = OtpHelper.HashOtpSha256(dto.Otp);
                if (!string.Equals(inputHash, user.EmailOtpHash, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new BaseCommentResponse(400, "OTP không đúng"));

                user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
                user.EmailOtpHash = null;
                user.EmailOtpExpiredAt = null;

                await _context.SaveChangesAsync();

                try
                {
                    var subject = "Thông báo đổi mật khẩu thành công";
                    var body =
$@"Xin chào {user.FullName},

Mật khẩu của bạn đã được đổi thành công.

Trân trọng.";
                    await _notificationService.SendEmailAsync(user.Email!, subject, body);
                }
                catch (Exception)
                {
                    // Email sending failed
                    return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi khi lấy mã OTP"));
                }

                return Ok(new BaseCommentResponse(200, "Đặt lại mật khẩu thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromQuery] ChangePasswordDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new BaseCommentResponse(401, "Người dùng chưa đăng nhập hoặc token không hợp lệ"));

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));

                if (!PasswordHelper.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                    return BadRequest(new BaseCommentResponse(400, "Mật khẩu hiện tại không chính xác"));

                user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();

                try
                {
                    var subject = "Thông báo đổi mật khẩu thành công";
                    var body =
$@"Xin chào {user.FullName},

Mật khẩu của bạn đã được thay đổi thành công theo yêu cầu.

Trân trọng.";
                    await _notificationService.SendEmailAsync(user.Email!, subject, body);
                }
                catch (Exception)
                {
                    // Email sending failed
                    return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi khi lấy mã OTP"));
                }

                return Ok(new BaseCommentResponse(200, "Đổi mật khẩu thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ!!!"));
            }
        }
    }
}
