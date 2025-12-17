using AutoMapper;
using SWP391.Contracts.Authentication;
using SWP391.Contracts.Campus;
using SWP391.Contracts.Category;
using SWP391.Contracts.Department;
using SWP391.Contracts.Location;
using SWP391.Contracts.Notification;
using SWP391.Contracts.Role;
using SWP391.Contracts.Ticket;
using SWP391.Contracts.User;
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
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore());

            // Map from RegisterRequestDto to User
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.UserCode, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // User to UserDto mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.DepartmentId,opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src =>src.Department != null? src.Department.DeptName: null));
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.Department,opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.TicketAssignedToNavigations, opt => opt.Ignore())
                .ForMember(dest => dest.TicketManagedByNavigations, opt => opt.Ignore())
                .ForMember(dest => dest.TicketRequesters, opt => opt.Ignore())
             
            ;

            // ? Location mappings with Campus information
            CreateMap<Location, LocationDto>()
                .ForMember(dest => dest.CampusName, opt => opt.MapFrom(src => src.Campus != null ? src.Campus.CampusName : string.Empty))
                .ForMember(dest => dest.CampusCode, opt => opt.MapFrom(src => src.Campus != null ? src.Campus.CampusCode : string.Empty));

            CreateMap<LocationRequestDto, Location>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Campus, opt => opt.Ignore())
                .ForMember(dest => dest.Tickets, opt => opt.Ignore());

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

            CreateMap<Role, RoleDto>();

            // Ticket mappings - Using CODES instead of IDs
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.RequesterCode, opt => opt.MapFrom(src => src.Requester.UserCode))
                .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => src.Requester.FullName))
                .ForMember(dest => dest.AssignedToCode, opt => opt.MapFrom(src => src.AssignedToNavigation != null ? src.AssignedToNavigation.UserCode : string.Empty))
                .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedToNavigation != null ? src.AssignedToNavigation.FullName : string.Empty))
                .ForMember(dest => dest.ManagedByCode, opt => opt.MapFrom(src => src.ManagedByNavigation != null ? src.ManagedByNavigation.UserCode : string.Empty))
                .ForMember(dest => dest.ManagedByName, opt => opt.MapFrom(src => src.ManagedByNavigation != null ? src.ManagedByNavigation.FullName : string.Empty))
                .ForMember(dest => dest.LocationCode, opt => opt.MapFrom(src => src.Location.LocationCode))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location.LocationName))
                .ForMember(dest => dest.CategoryCode, opt => opt.MapFrom(src => src.Category.CategoryCode))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.ContactPhone ?? string.Empty))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note ?? string.Empty))
                .ForMember(dest => dest.RatingComment, opt => opt.MapFrom(src => src.RatingComment ?? string.Empty))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl ?? string.Empty));

            CreateMap<User, UserProfileDto>();

            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.TicketCode, opt => opt.MapFrom(src => src.TicketCode ?? string.Empty));

            CreateMap<Campus, CampusDto>()
                .ForMember(dest => dest.CampusId, opt => opt.MapFrom(src => src.Id.ToString()));
        }
    }
}