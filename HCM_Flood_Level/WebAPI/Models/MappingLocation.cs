using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingLocation : Profile
    {
        public MappingLocation()
        {
            CreateMap<Location, ManageLocationDTO>()
                .ForMember(a => a.LocationId, a => a.MapFrom(b => b.LocationId))
                .ForMember(a => a.LocationName, a => a.MapFrom(b => b.LocationName));
        }
    }
}
