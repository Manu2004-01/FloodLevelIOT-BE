using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DBContext
{
    public class EventsDBContext : DbContext
    {
        public EventsDBContext(DbContextOptions<EventsDBContext> options) : base(options)
        {
        }

        public virtual DbSet<SensorReading> SensorReadings { get; set; }
        public virtual DbSet<Alert> Alerts { get; set; }
        public virtual DbSet<AlertLog> AlertLogs { get; set; }
        public virtual DbSet<FloodEvent> FloodEvents { get; set; }
        public virtual DbSet<Sensor> Sensors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureColumnNames(modelBuilder);

            modelBuilder.Entity<SensorReading>().ToTable("sensorreadings");
            modelBuilder.Entity<Alert>().ToTable("alerts");
            modelBuilder.Entity<AlertLog>().ToTable("alertlogs");
            modelBuilder.Entity<FloodEvent>().ToTable("floodevents");
            modelBuilder.Entity<Sensor>().ToTable("sensors");

            // Link SensorReading -> Sensor (application-level FK mapping)
            modelBuilder.Entity<SensorReading>()
                .HasOne<Sensor>()
                .WithMany()
                .HasForeignKey(sr => sr.SensorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AlertLog>()
                .HasOne(al => al.Alert)
                .WithMany()
                .HasForeignKey(al => al.AlertId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureColumnNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorReading>(entity =>
            {
                // Explicitly set primary key because property name is 'ReadingId'
                entity.HasKey(e => e.ReadingId);
                entity.Property(e => e.ReadingId).HasColumnName("reading_id");
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.WaterLevel).HasColumnName("water_level_cm");
                entity.Property(e => e.Battery).HasColumnName("battery_percent");
                entity.Property(e => e.SignalStrength).HasColumnName("signal_strength");
                entity.Property(e => e.RecordAt).HasColumnName("recorded_at");
            });

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => e.AlertId);
                entity.Property(e => e.AlertId).HasColumnName("alert_id");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.LevelId).HasColumnName("level_id");
                entity.Property(e => e.AlertMessage).HasColumnName("alert_message");
                entity.Property(e => e.IssuedAt).HasColumnName("issued_at");
            });

            modelBuilder.Entity<AlertLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.LogId).HasColumnName("log_id");
                entity.Property(e => e.AlertId).HasColumnName("alert_id");
                entity.Property(e => e.Channel).HasColumnName("channel");
                entity.Property(e => e.SentAt).HasColumnName("sent_at");
                entity.Property(e => e.LogStatus).HasColumnName("status");
            });

            modelBuilder.Entity<FloodEvent>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventId).HasColumnName("event_id");
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.MaxWaterLevel).HasColumnName("max_water_level");
                entity.Property(e => e.Severity).HasColumnName("severity");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.InstalledBy).HasColumnName("installed_by");
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
        }
    }
}