using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingRequest : Profile
    {
        public MappingRequest() 
        {
            CreateMap<StaffCreateRequestDTO, MaintenanceRequest>()
                .ForMember(a => a.SensorId, a => a.MapFrom(b => b.SensorId))
                .ForMember(a => a.PriorityId, a => a.MapFrom(b => b.Priorityid))
                .ForMember(a => a.AssignedTechnicianTo, a => a.MapFrom(b => b.AssignedTechnicianTo))
               .ForMember(a => a.Description, a => a.MapFrom(b => b.Description))
               .ForMember(a => a.Deadline, a => a.MapFrom(b => b.Deadline))
               .ForMember(a => a.Note, a => a.MapFrom(b => b.Note))
               .ReverseMap();

            CreateMap<MaintenanceRequest, RequestDTO>()
                .ForMember(a => a.RequestId, a => a.MapFrom(b => b.RequestId))
                .ForMember(a => a.SensorName, a => a.MapFrom(b => b.Sensor.SensorName))
                .ForMember(a => a.Priority, a => a.MapFrom(b => b.Priority.DisplayName))
                .ForMember(a => a.AssignedTechnicianTo, a => a.MapFrom(b => b.AssignedTechnician.FullName));
        }
    }
}
