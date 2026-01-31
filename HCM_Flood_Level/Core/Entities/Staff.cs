namespace Core.Entities
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string FullName { get; set; }
        public string StaffAccName { get; set; } // account username
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
