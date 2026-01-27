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
        public virtual DbSet<DigitalSign> DigitalSigns { get; set; }
        public virtual DbSet<FloodLevel> FloodLevels { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Sensor> Sensors { get; set; }
        public virtual DbSet<Priority> Priorities { get; set; }
        public virtual DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureColumnNames(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<Area>().ToTable("areas");
            modelBuilder.Entity<Location>().ToTable("locations");
            modelBuilder.Entity<Sensor>().ToTable("sensors");
            modelBuilder.Entity<FloodLevel>().ToTable("floodlevels");
            modelBuilder.Entity<DigitalSign>().ToTable("digitalsigns");
            modelBuilder.Entity<Priority>().ToTable("priorities");
            modelBuilder.Entity<MaintenanceRequest>().ToTable("maintenancerequests");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Sensor>()
                .HasIndex(s => s.SensorCode)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Location>()
                .HasOne(l => l.Area)
                .WithMany()
                .HasForeignKey(l => l.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sensor>()
                .HasOne(s => s.Location)
                .WithMany()
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DigitalSign>()
                .HasOne(d => d.Location)
                .WithMany()
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Sensor)
                .WithMany()
                .HasForeignKey(m => m.SensorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Priority)
                .WithMany()
                .HasForeignKey(m => m.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.AssignedUser)
                .WithMany()
                .HasForeignKey(m => m.AssignTo)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureColumnNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.RoleName).HasColumnName("role_name");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FullName).HasColumnName("full_name");
                entity.Property(e => e.Username).HasColumnName("user_name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Area>(entity =>
            {
                entity.Property(e => e.AreaId).HasColumnName("area_id");
                entity.Property(e => e.AreaName).HasColumnName("area_name");
                entity.Property(e => e.Description).HasColumnName("description");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.AreaId).HasColumnName("area_id");
                entity.Property(e => e.LocationName).HasColumnName("location_name");
                entity.Property(e => e.Latitude).HasColumnName("latitude");
                entity.Property(e => e.Longitude).HasColumnName("longitude");
                entity.Property(e => e.Address).HasColumnName("road_name");
            });

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.SensorCode).HasColumnName("sensor_code");
                entity.Property(e => e.SensorName).HasColumnName("sensor_name");
                entity.Property(e => e.SensorType).HasColumnName("sensor_type");
                entity.Property(e => e.SensorStatus).HasColumnName("status");
                entity.Property(e => e.InstalledAt).HasColumnName("installed_at");
            });

            modelBuilder.Entity<FloodLevel>(entity =>
            {
                entity.HasKey(e => e.LevelId);
                entity.Property(e => e.LevelId).HasColumnName("level_id");
                entity.Property(e => e.LevelName).HasColumnName("level_name");
                entity.Property(e => e.Min).HasColumnName("min_cm");
                entity.Property(e => e.Max).HasColumnName("max_cm");
                entity.Property(e => e.Color).HasColumnName("color_code");
            });

            modelBuilder.Entity<DigitalSign>(entity =>
            {
                entity.HasKey(e => e.SignId);
                entity.Property(e => e.SignId).HasColumnName("sign_id");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.SignCode).HasColumnName("sign_code");
                entity.Property(e => e.SignStatus).HasColumnName("status");
            });

            modelBuilder.Entity<Priority>(entity =>
            {
                entity.Property(e => e.PriorityId).HasColumnName("priority_id");
                entity.Property(e => e.Name).HasColumnName("display_name");
            });

            modelBuilder.Entity<MaintenanceRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.Property(e => e.RequestId).HasColumnName("request_id");
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.PriorityId).HasColumnName("priority_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.AssignTo).HasColumnName("assigned_to");
                entity.Property(e => e.Note).HasColumnName("note");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });
        }
    }
}
