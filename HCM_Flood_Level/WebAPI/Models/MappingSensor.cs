using AutoMapper;
using Core.DTOs.Locations;
using Core.DTOs.Sensor;
using Core.Entities;
using System;

namespace WebAPI.Models
{
    public class MappingSensor : Profile
    {
        public MappingSensor()
        {
            CreateMap<Sensor, ManageSensorDTO>()
                .ForMember(a => a.SensorId, a => a.MapFrom(b => b.SensorId))
                .ForMember(a => a.SensorName, a => a.MapFrom(b => b.SensorName))
                .ForMember(a => a.LocationName, a => a.MapFrom(b => b.Location != null ? b.Location.LocationName : null))
                .ForMember(a => a.AreaName, a => a.MapFrom(b => b.Location != null && b.Location.Area != null ? b.Location.Area.AreaName : null))
                .ForMember(a => a.SensorStatus, a => a.MapFrom(b => b.Status))
                .ForMember(a => a.InstalledAt, a => a.MapFrom(b => b.InstalledAt));

            CreateMap<Sensor, SensorDTO>()
                .ForMember(a => a.SensorId, a => a.MapFrom(b => b.SensorId))
                .ForMember(a => a.SensorCode, a => a.MapFrom(b => b.SensorCode))
                .ForMember(a => a.Protocol, a => a.MapFrom(b => b.Protocol))
                .ForMember(a => a.WarrantyDate, a => a.MapFrom(b => b.CreatedAt))
                .ForMember(a => a.SensorType, a => a.MapFrom(b => b.SensorType))
                .ForMember(a => a.WarningThreshold, a => a.MapFrom(b => b.WarningThreshold ?? 0))
                .ForMember(a => a.DangerThreshold, a => a.MapFrom(b => b.DangerThreshold ?? 0))
                .ForMember(a => a.MaxLevel, a => a.MapFrom(b => b.MaxLevel))
                .ForMember(a => a.Battery, a => a.MapFrom(b => 0))
                .ForMember(a => a.InstalledAt, a => a.MapFrom(b => b.InstalledAt ?? b.CreatedAt))
                .ForMember(a => a.CommissionedAt, a => a.MapFrom(b => b.CreatedAt))
                .ForMember(a => a.InstalledByStaff, a => a.MapFrom(b => b.InstalledBy != null ? b.InstalledByStaff.FullName : string.Empty))
                .ForMember(a => a.Location, a => a.MapFrom(b => b.Location));

            CreateMap<Location, LocationDetailDTO>()
                .ForMember(a => a.LocationId, a => a.MapFrom(b => b.LocationId))
                .ForMember(a => a.LocationName, a => a.MapFrom(b => b.LocationName))
                .ForMember(a => a.Latitude, a => a.MapFrom(b => b.Latitude))
                .ForMember(a => a.Longitude, a => a.MapFrom(b => b.Longitude))
                .ForMember(a => a.AreaId, a => a.MapFrom(b => b.AreaId))
                .ForMember(a => a.AreaName, a => a.MapFrom(b => b.Area != null ? b.Area.AreaName : null));

            CreateMap<CreateSensorDTO, Sensor>()
                .ForMember(a => a.LocationId, a => a.MapFrom(b => b.LocationId))
                .ForMember(a => a.InstalledBy, a => a.MapFrom(b => b.InstalledBy))
                .ForMember(a => a.Specification, a => a.MapFrom(b => b.Specification))
                .ForMember(a => a.SensorCode, a => a.MapFrom(b => b.SensorCode))
                .ForMember(a => a.SensorName, a => a.MapFrom(b => b.SensorName))
                .ForMember(a => a.Protocol, a => a.MapFrom(b => b.Protocol))
                .ForMember(a => a.SensorType, a => a.MapFrom(b => b.SensorType))
                .ForMember(a => a.WarningThreshold, a => a.MapFrom(b => b.MinThreshold))
                .ForMember(a => a.DangerThreshold, a => a.MapFrom(b => b.MaxThreshold))
                .ForMember(a => a.MaxLevel, a => a.MapFrom(b => b.MaxLevel));

            CreateMap<UpdateSensorDTO, Sensor>()
                .ForMember(a => a.SensorId, a => a.Ignore())
                .ForMember(a => a.InstalledAt, a => a.Ignore())
                .ForMember(a => a.Location, a => a.Ignore())
                .ForMember(a => a.SensorCode, a => a.MapFrom(b => b.SensorCode))
                .ForMember(a => a.SensorName, a => a.MapFrom(b => b.SensorName))
                .ForMember(a => a.Protocol, a => a.MapFrom(b => b.Protocol))
                .ForMember(a => a.SensorType, a => a.MapFrom(b => b.SensorType))
                .ForMember(a => a.InstalledBy, a => a.MapFrom(b => b.InstalledBy))
                .ForMember(a => a.Specification, a => a.MapFrom(b => b.Specification))
                .ForMember(a => a.WarningThreshold, a => a.MapFrom(b => b.MinThreshold))
                .ForMember(a => a.DangerThreshold, a => a.MapFrom(b => b.MaxThreshold))
                .ForMember(a => a.MaxLevel, a => a.MapFrom(b => b.MaxLevel))
                .ForMember(a => a.LocationId, a => a.MapFrom(b => b.LocationId))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}