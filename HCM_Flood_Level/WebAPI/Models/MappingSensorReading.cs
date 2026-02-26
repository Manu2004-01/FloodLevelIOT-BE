using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingSensorReading : Profile
    {
        public MappingSensorReading()
        {
            CreateMap<SensorReading, SensorReadingDTO>()
                .ForMember(d => d.ReadingId, o => o.MapFrom(s => s.ReadingId))
                .ForMember(d => d.SensorId, o => o.MapFrom(s => s.SensorId))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
                .ForMember(d => d.WaterLevelCm, o => o.MapFrom(s => s.WaterLevelCm))
                .ForMember(d => d.BatteryPercent, o => o.MapFrom(s => s.BatteryPercent))
                .ForMember(d => d.SignalStrength, o => o.MapFrom(s => s.SignalStrength))
                .ForMember(d => d.RecordedAt, o => o.MapFrom(s => s.RecordedAt));
        }
    }
}
