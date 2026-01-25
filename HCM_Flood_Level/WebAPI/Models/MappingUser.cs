using AutoMapper;
using Core.DTOs.Admin;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingUser : Profile
    {
        public MappingUser()
        {
            CreateMap<User, ManageAccDTO>()
                .ForMember(a => a.UserId, a => a.MapFrom(b => b.UserId))
                .ForMember(a => a.RoleName, a => a.MapFrom(b => b.Role != null ? b.Role.RoleName : string.Empty));

            CreateMap<User, AccDTO>()
                .ForMember(a => a.UserId, a => a.MapFrom(b => b.UserId))
                .ForMember(a => a.RoleName, a => a.MapFrom(b => b.Role != null ? b.Role.RoleName : string.Empty));

            CreateMap<CreateAccDTO, User>()
                .ForMember(a => a.RoleId, a => a.MapFrom(b => b.RoleId))
                .ReverseMap();
        }
    }
}
