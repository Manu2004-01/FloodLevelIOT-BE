using System;

namespace Core.Entities
{
    public class Citizen
    {
        public int CitizenId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public int AreaId { get; set; }
        public Area Area { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


