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
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Sensor> Sensors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Globally ignore Location in this context as it's a management entity.
            modelBuilder.Ignore<Location>();

            ConfigureColumnNames(modelBuilder);

            modelBuilder.Entity<SensorReading>().ToTable("sensorreadings");
            modelBuilder.Entity<History>().ToTable("history");
            modelBuilder.Entity<Report>().ToTable("report");
            modelBuilder.Entity<Sensor>().ToTable("sensors");

            // In the events database we only care about sensor metadata and readings.
            // Ignore maintenance- and management-related navigations to avoid pulling
            // those entities (and their complex relationships) into this model.
            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.Ignore(s => s.Technician);
                entity.Ignore(s => s.MaintenanceRequests);
                entity.Ignore(s => s.MaintenanceSchedules);
                entity.Ignore(s => s.SensorReadings);
            });
            
            // SensorReading only stores raw data columns; do not model FK to Sensor here
            modelBuilder.Entity<SensorReading>(entity =>
            {
                entity.Ignore(sr => sr.Sensor);
            });
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
                entity.Property(e => e.WaterLevelCm).HasColumnName("water_level_cm");
                entity.Property(e => e.BatteryPercent).HasColumnName("battery_percent");
                entity.Property(e => e.SignalStrength).HasColumnName("signal_strength");
                entity.Property(e => e.RecordedAt).HasColumnName("recorded_at").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<History>(entity =>
            {
                entity.HasKey(e => e.HistoryId);
                entity.Property(e => e.HistoryId).HasColumnName("history_id");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.MaxWaterLevel).HasColumnName("max_water_level");
                entity.Property(e => e.Severity).HasColumnName("severity");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.ReportId);
                entity.Property(e => e.ReportId).HasColumnName("report_id");
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.Property(e => e.SensorId).HasColumnName("sensor_id");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");
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
