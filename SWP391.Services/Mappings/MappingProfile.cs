using AutoMapper;
using SWP391.Contracts;
using SWP391.Contracts.Authentication;
using SWP391.Contracts.Department;
using SWP391.Contracts.Location;
using SWP391.Repositories.Models;

namespace SWP391.Services.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map from User entity to AuthResponseDto
            CreateMap<User, AuthResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.Token, opt => opt.Ignore())  // Set manually
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore()); // Set manually

            // Map from RegisterRequestDto to User
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.UserCode, opt => opt.Ignore())  // Generated in service
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())  // Hashed in service
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<Location, LocationDto>()
                .ForMember(dest => dest.LocationCode, opt => opt.MapFrom(src => src.LocationCode))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.LocationName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<LocationRequestDto, Location>()
                .ForMember(dest => dest.LocationCode, opt => opt.MapFrom(src => src.LocationCode))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.LocationName));

            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryRequestDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Tickets, opt => opt.Ignore());

            CreateMap<Department, DepartmentDto>();
            CreateMap<DepartmentRequestDto, Department>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Users, opt => opt.Ignore());
        }
    }
}