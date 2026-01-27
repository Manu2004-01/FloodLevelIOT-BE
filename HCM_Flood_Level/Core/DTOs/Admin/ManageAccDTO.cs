using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Admin
{
    public class ManageAccDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }

        public string RoleName { get; set; }
    }

    public class AccDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string RoleName { get; set; }
    }

    public class CreateAccDTO
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateAccDTO
    {
        public int? RoleId { get; set; }
        public string? Status { get; set; }
    }
}
