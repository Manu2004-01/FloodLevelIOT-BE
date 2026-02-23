using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingMaintenanceSchedule : Profile
    {
        public MappingMaintenanceSchedule() 
        {
            CreateMap<CreateMaintenanceScheduleDTO, MaintenanceSchedule>()
                .ForMember(a => a.SensorId, a => a.MapFrom(b => b.SensorId))
                .ForMember(a => a.ScheduleType, a => a.MapFrom(b => b.ScheduleType))
                .ForMember(a => a.ScheduleMode, a => a.MapFrom(b => b.ScheduleMode))
                .ForMember(a => a.StartDate, a => a.MapFrom(b => b.StartDate))
                .ForMember(a => a.EndDate, a => a.MapFrom(b => b.EndDate))
                .ForMember(a => a.AssignedTechnicianId, a => a.MapFrom(b => b.AssignedStaffId))
                .ForMember(a => a.Note, a => a.MapFrom(b => b.Note))
                .ReverseMap();

            CreateMap<MaintenanceSchedule, MaintenanceScheduleDTO>()
                .ForMember(a => a.SensorName, a => a.MapFrom(b => b.Sensor.SensorName))
                .ForMember(a => a.AssignedStaff, a => a.MapFrom(b => b.AssignedTechnician.FullName));
        }
    }
}
