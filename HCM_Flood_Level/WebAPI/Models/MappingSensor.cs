using AutoMapper;
using Core.DTOs.Sensor;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingSensor : Profile
    {
        public MappingSensor()
        {
            CreateMap<Sensor, SensorDto>()
                 .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                 .ForMember(dest => dest.SensorId, opt => opt.MapFrom(src => src.SensorId))
                 .ForMember(dest => dest.SensorCode, opt => opt.MapFrom(src => src.SensorCode))
                 .ForMember(dest => dest.SensorName, opt => opt.MapFrom(src => src.SensorName))
                 .ForMember(dest => dest.SensorType, opt => opt.MapFrom(src => src.SensorType))
                 .ForMember(dest => dest.SensorStatus, opt => opt.MapFrom(src => src.SensorStatus))
                 .ForMember(dest => dest.InstalledAt, opt => opt.MapFrom(src => src.InstalledAt))
                 .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.LocationId));
                 //.ForMember(dest => dest.MinThreshold, opt => opt.MapFrom(src => src.MinThreshold))
                 //.ForMember(dest => dest.MaxThreshold, opt => opt.MapFrom(src => src.MaxThreshold))
                 //.ForMember(dest => dest.ThresholdType, opt => opt.MapFrom(src => src.ThresholdType));

            CreateMap<SensorCreateDto, Sensor>()
                .ForMember(dest => dest.SensorId, opt => opt.Ignore()) 
                .ForMember(dest => dest.Location, opt => opt.Ignore()) 
                .ForMember(dest => dest.SensorCode, opt => opt.MapFrom(src => src.SensorCode))
                .ForMember(dest => dest.SensorName, opt => opt.MapFrom(src => src.SensorName))
                .ForMember(dest => dest.SensorType, opt => opt.MapFrom(src => src.SensorType))
                .ForMember(dest => dest.SensorStatus, opt => opt.MapFrom(src => src.SensorStatus))
                .ForMember(dest => dest.InstalledAt, opt => opt.MapFrom(src => src.InstalledAt))
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.LocationId));

            CreateMap<SensorUpdateDto, Sensor>()
                .ForMember(dest => dest.SensorId, opt => opt.Ignore()) 
                .ForMember(dest => dest.SensorCode, opt => opt.Ignore()) 
                .ForMember(dest => dest.InstalledAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.Location, opt => opt.Ignore()) 
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Location, LocationDto>()
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.LocationId))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.LocationName))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.AreaId, opt => opt.MapFrom(src => src.AreaId))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Area != null ? src.Area.AreaName : null));

     
            CreateMap<Area, AreaDto>()
                .ForMember(dest => dest.AreaId, opt => opt.MapFrom(src => src.AreaId))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.AreaName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}