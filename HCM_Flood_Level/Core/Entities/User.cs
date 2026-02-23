using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<MaintenanceRequest> MaintenanceRequestsCreated { get; set; }
        public ICollection<MaintenanceRequest> MaintenanceRequestsAssigned { get; set; }
        public ICollection<MaintenanceSchedule> MaintenanceSchedulesCreated { get; set; }
        public ICollection<MaintenanceSchedule> MaintenanceSchedulesAssigned { get; set; }
        public ICollection<Sensor> SensorsMaintained { get; set; }
    }
}
