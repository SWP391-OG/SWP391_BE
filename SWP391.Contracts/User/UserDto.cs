using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Contracts.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public int? DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }
    }

    public class UserProfileDto
    {
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

    }
    public class UserUpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UserCreateDto
    {
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UserUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public string? PasswordHash { get; set; } = string.Empty;
    }

    public class UserStatusUpdateDto
    {
        public int UserId { get; set; }
        public string Status { get; set; } = string.Empty;
    }


}
