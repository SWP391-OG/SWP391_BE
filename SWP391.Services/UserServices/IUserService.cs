using SWP391.Contracts.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.UserServices
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserProfileDto?> GetUseProfileByUserIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto?> GetUserByUserCodeAsync(string userCode);
        Task<(bool Success, string Message)> UpdateProfileUserAsync(int userId, UserUpdateProfileDto userDto);
        Task<(bool Success, string Message, UserDto Data)> CreateUserAsync(UserCreateDto userDto);
        Task<(bool Success, string Message)> UpdateUserAsync(int userId, UserUpdateDto userDto);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId);
        Task<(bool Success, string Message)> UpdateUserStatusAsync(UserStatusUpdateDto userDto);
    }
}
