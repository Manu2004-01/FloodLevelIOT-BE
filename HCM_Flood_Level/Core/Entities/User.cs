namespace Core.Entities
{
    public class User
    {
        public int UserId { get; set; } 
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
