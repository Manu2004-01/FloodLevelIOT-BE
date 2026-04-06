using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingUser : Profile
    {
        public MappingUser()
        {
            CreateMap<User, UserSummaryDTO>()
                .ForMember(a => a.UserId, a => a.MapFrom(b => b.UserId))
                .ForMember(a => a.Status, a => a.MapFrom(b => b.IsActive))
                .ForMember(a => a.RoleName, a => a.MapFrom(b => b.Role != null ? b.Role.RoleName : string.Empty))
                .ForMember(a => a.Email, a => a.MapFrom(b => b.Email));

            CreateMap<User, UserDTO>()
                .ForMember(a => a.UserId, a => a.MapFrom(b => b.UserId))
                .ForMember(a => a.Status, a => a.MapFrom(b => b.IsActive))
                .ForMember(a => a.RoleName, a => a.MapFrom(b => b.Role != null ? b.Role.RoleName : string.Empty))
                .ForMember(a => a.Email, a => a.MapFrom(b => b.Email))
                .ForMember(d => d.Schedule, opt => opt.Ignore())
                .ForMember(d => d.Request, opt => opt.Ignore());

            CreateMap<CreateUserDTO, User>()
                .ForMember(a => a.PasswordHash, a => a.Ignore()) 
                .ForMember(a => a.FullName, a => a.MapFrom(b => b.FullName))
                .ReverseMap();

            CreateMap<UpdateUserDTO, User>()
                .ForMember(a => a.RoleId, a => a.MapFrom(b => b.RoleId))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateProfileDTO, User>()
                .ForMember(a => a.FullName, a => a.MapFrom(b => b.FullName))
                .ForMember(a => a.PhoneNumber, a => a.MapFrom(b => b.PhoneNumber))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
