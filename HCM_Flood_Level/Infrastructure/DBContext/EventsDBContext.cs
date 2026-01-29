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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureColumnNames(modelBuilder);

            modelBuilder.Entity<SensorReading>().ToTable("sensorreadings");
            modelBuilder.Entity<Alert>().ToTable("alerts");
            modelBuilder.Entity<AlertLog>().ToTable("alertlogs");

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
        }
    }
}