using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DBContext
{
    public class EventsDBContext : DbContext, IEventsDBContext
    {
        public EventsDBContext(DbContextOptions<EventsDBContext> options) : base(options)
        {
        }

        public virtual DbSet<SensorReading> SensorReadings { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresEnum<Severity>();

            modelBuilder.Ignore<Location>();

            ConfigureColumnNames(modelBuilder);

            modelBuilder.Entity<SensorReading>().ToTable("sensorreadings");
            modelBuilder.Entity<History>().ToTable("history");
            modelBuilder.Entity<Report>().ToTable("report");

            modelBuilder.Entity<SensorReading>(entity =>
            {
                entity.Ignore(sr => sr.Sensor);
            });
        }

        private static void ConfigureColumnNames(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorReading>(entity =>
            {
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
                entity.Property(e => e.Severity)
                    .HasColumnName("severity")
                    .HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.ReportId);
                entity.Property(e => e.ReportId).HasColumnName("report_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ForecastRiskLevel).HasColumnName("forecast_risk_level");
                entity.Property(e => e.ForecastDataJson).HasColumnName("forecast_data_json");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").ValueGeneratedOnAdd();
            });
        }
    }
}
