using Core.DTOs.Auth;
using Core.Interfaces;
using Core.Services;
using Infrastructure.DBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Errors;

namespace WebAPI.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ManageDBContext _context;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthController(ManageDBContext context, ITokenService tokenService, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
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

                if (string.IsNullOrWhiteSpace(login.Username))
                    return BadRequest(new BaseCommentResponse(400, "Tên đăng nhập là bắt buộc"));

                if (string.IsNullOrWhiteSpace(login.Password))
                    return BadRequest(new BaseCommentResponse(400, "Mật khẩu là bắt buộc"));

                var log = await _context.Staffs
                    .Include(l => l.Role)
                    .FirstOrDefaultAsync(l => l.StaffAccName == login.Username);

                if (log == null)
                    return Unauthorized(new BaseCommentResponse(401, "Tên đăng nhập hoặc mật khẩu không đúng"));

                if(!PasswordHelper.VerifyPassword(login.Password, log.PasswordHash))
                    return Unauthorized(new BaseCommentResponse(401, "Mật khẩu không đúng"));
                
                var roleName = log.Role?.RoleName ?? string.Empty; 

                var token = _tokenService.CreateToken(log, roleName);

                return Ok(new {token = token});
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
    }
}
