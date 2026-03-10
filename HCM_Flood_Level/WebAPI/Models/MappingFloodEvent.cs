using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingHistory : Profile
    {
        public MappingHistory()
        {
            CreateMap<History, HistoryDTO>()
                .ForMember(d => d.HistoryId, o => o.MapFrom(s => s.HistoryId))
                .ForMember(d => d.LocationId, o => o.MapFrom(s => s.LocationId))
                .ForMember(d => d.StartTime, o => o.MapFrom(s => s.StartTime))
                .ForMember(d => d.EndTime, o => o.MapFrom(s => s.EndTime))
                .ForMember(d => d.MaxWaterLevel, o => o.MapFrom(s => s.MaxWaterLevel))
                .ForMember(d => d.Severity, o => o.MapFrom(s => s.Severity))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt));
        }
    }
}
