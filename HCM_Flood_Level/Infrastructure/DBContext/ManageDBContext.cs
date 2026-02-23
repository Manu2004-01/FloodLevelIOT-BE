using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DBContext
{
    public class ManageDBContext : DbContext
    {
        public ManageDBContext(DbContextOptions<ManageDBContext> options) : base(options)
        {
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Sensor> Sensors { get; set; }
        public virtual DbSet<Priority> Priorities { get; set; }
        public virtual DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public virtual DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
        public virtual DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<Area>().ToTable("areas");
            modelBuilder.Entity<Location>().ToTable("locations");
            modelBuilder.Entity<Sensor>().ToTable("sensors");
            modelBuilder.Entity<Priority>().ToTable("priorities");
            modelBuilder.Entity<MaintenanceRequest>().ToTable("maintenancerequests");
            modelBuilder.Entity<MaintenanceTask>().ToTable("maintenancetasks");
            modelBuilder.Entity<MaintenanceSchedule>().ToTable("maintenanceschedules");

            modelBuilder.Entity<Sensor>()
                .HasIndex(s => s.SensorCode)
                .IsUnique();

            // Configure relationships BEFORE ConfigureColumnNames so navigations are bound to FKs first
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Area)
                .WithMany(a => a.Locations)
                .HasForeignKey(l => l.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sensor>()
                .HasOne(s => s.Location)
                .WithMany(l => l.Sensors)
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sensor>()
                .HasOne(s => s.Technician)
                .WithMany(u => u.SensorsMaintained)
                .HasForeignKey(s => s.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.AssignedTechnician)
                .WithMany(u => u.MaintenanceRequestsAssigned)
                .HasForeignKey(m => m.AssignedTechnicianTo)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MaintenanceRequest_AssignedTechnician");

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.CreatedByStaff)
                .WithMany(u => u.MaintenanceRequestsCreated)
                .HasForeignKey(m => m.CreatedByStaffId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MaintenanceRequest_CreatedByStaff");

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Priority)
                .WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(m => m.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Sensor)
                .WithMany(s => s.MaintenanceRequests)
                .HasForeignKey(m => m.SensorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceSchedule>()
                .HasOne(s => s.Sensor)
                .WithMany(s => s.MaintenanceSchedules)
                .HasForeignKey(s => s.SensorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceSchedule>()
                .HasOne(s => s.AssignedTechnician)
                .WithMany(u => u.MaintenanceSchedulesAssigned)
                .HasForeignKey(s => s.AssignedTechnicianId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MaintenanceSchedule_AssignedTechnician");

            modelBuilder.Entity<MaintenanceSchedule>()
                .HasOne(s => s.CreatedByStaff)
                .WithMany(u => u.MaintenanceSchedulesCreated)
                .HasForeignKey(s => s.CreatedByStaffId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MaintenanceSchedule_CreatedByStaff");

            // Ensure SensorReading has key if mapped in this context
            if (modelBuilder.Model.GetEntityTypes().Any(e => e.ClrType == typeof(Core.Entities.SensorReading)))
            {
                modelBuilder.Entity<SensorReading>(entity =>
                {
                    entity.HasKey(e => e.ReadingId);
                });
            }

            modelBuilder.Entity<MaintenanceTask>()
                .HasOne(t => t.AssignedTechnician)
                .WithMany()
                .HasForeignKey(t => t.AssignedTechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceTask>()
                .HasOne(t => t.Priority)
                .WithMany(p => p.MaintenanceTasks)
                .HasForeignKey(t => t.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceTask>()
                .HasOne(t => t.Request)
                .WithMany()
                .HasForeignKey(t => t.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceTask>()
                .HasOne(t => t.Sensor)
                .WithMany(s => s.MaintenanceTasks)
                .HasForeignKey(t => t.SensorId)
                .OnDelete(DeleteBehavior.Restrict);

            ConfigureColumnNames(modelBuilder);
        }

        private static void ConfigureColumnNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FullName).HasColumnName("full_name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.RoleName).HasColumnName("role_name");
            });


            modelBuilder.Entity<Area>(entity =>
            {
                entity.Property(e => e.AreaId).HasColumnName("area_id");
                entity.Property(e => e.AreaName).HasColumnName("area_name");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.AreaId).HasColumnName("area_id");
                entity.Property(e => e.LocationName).HasColumnName("location_name");
                entity.Property(e => e.Latitude).HasColumnName("latitude");
                entity.Property(e => e.Longitude).HasColumnName("longitude");
                entity.Property(e => e.RoadName).HasColumnName("road_name");
            });

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.TechnicianId).HasColumnName("technician_id");
                entity.Property(e => e.SensorCode).HasColumnName("sensor_code");
                entity.Property(e => e.SensorName).HasColumnName("sensor_name");
                entity.Property(e => e.SensorType).HasColumnName("sensor_type");
                entity.Property(e => e.Protocol).HasColumnName("protocol");
                entity.Property(e => e.Specification).HasColumnName("specification");
                entity.Property(e => e.InstalledAt).HasColumnName("installed_at");
                entity.Property(e => e.WarningThreshold).HasColumnName("warning_threshold");
                entity.Property(e => e.DangerThreshold).HasColumnName("danger_threshold");
                entity.Property(e => e.MaxLevel).HasColumnName("max_level");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<MaintenanceRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.Property(e => e.RequestId).HasColumnName("request_id");
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.PriorityId).HasColumnName("priority_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Deadline).HasColumnName("deadline");
                entity.Property(e => e.AssignedTechnicianTo).HasColumnName("assigned_technician_to");
                entity.Property(e => e.CreatedByStaffId).HasColumnName("created_by_staff_id");
                entity.Property(e => e.Note).HasColumnName("note");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
                entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<MaintenanceTask>(entity =>
            {
                entity.HasKey(e => e.TaskId);
                entity.Property(e => e.TaskId).HasColumnName("task_id");
                entity.Property(e => e.RequestId).HasColumnName("request_id");
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.PriorityId).HasColumnName("priority_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.AssignedTechnicianId).HasColumnName("assigned_technician_id");
                entity.Property(e => e.Deadline).HasColumnName("deadline");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.StartedAt).HasColumnName("started_at");
                entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<MaintenanceSchedule>(entity =>
            {
                entity.HasKey(e => e.ScheduleId);
                entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.ScheduleType).HasColumnName("schedule_type");
                entity.Property(e => e.ScheduleMode).HasColumnName("schedule_mode");
                entity.Property(e => e.StartDate).HasColumnName("start_date");
                entity.Property(e => e.EndDate).HasColumnName("end_date");
                entity.Property(e => e.CreatedByStaffId).HasColumnName("created_by_staff_id");
                entity.Property(e => e.AssignedTechnicianId).HasColumnName("assigned_technician_id");
                entity.Property(e => e.Note).HasColumnName("note");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });
        }
    }
}
