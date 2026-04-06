using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.DTOs
{
    /// <summary>Danh sách người dùng (không kèm lịch / yêu cầu bảo trì).</summary>
    public class UserSummaryDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }
        public string RoleName { get; set; }
    }

    /// <summary>Chi tiết người dùng từ Staff API; schedule/request chỉ được điền khi RoleName là Technician.</summary>
    public class UserDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }

        public string RoleName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("schedule")]
        public List<ScheduleDTO>? Schedule { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("request")]
        public List<RequestDTO>? Request { get; set; }
    }

    public class UserDetailDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool Status { get; set; }
        public string RoleName { get; set; }
    }

    public class CreateUserDTO
    {
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class UpdateUserDTO
    {
        public int? RoleId { get; set; }
        public bool? Status { get; set; }
    }

    public class UpdateProfileDTO
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ProfileDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
