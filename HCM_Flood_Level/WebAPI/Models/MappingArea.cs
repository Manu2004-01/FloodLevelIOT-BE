using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingArea : Profile
    {
        public MappingArea() 
        {
            CreateMap<Area, ManageAreaDTO>()
                .ForMember(a => a.AreaId, a => a.MapFrom(b => b.AreaId))
                .ForMember(a => a.AreaName, a => a.MapFrom(b => b.AreaName));
        }
    }
}
