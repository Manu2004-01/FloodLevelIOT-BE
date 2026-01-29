using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Admin
{
    public class ManageStaffDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }

        public string RoleName { get; set; }
    }

    public class StaffDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool Status { get; set; }
        public string RoleName { get; set; }
    }

    public class CreateStaffDTO
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateStaffDTO
    {
        public int? RoleId { get; set; }
        public bool? Status { get; set; }
    }
}
