using AutoMapper;
using SWP391.Contracts.User;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;

namespace SWP391.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return userDtos;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        /// <summary>
        /// Get profile by user code
        /// </summary>
        public async Task<UserProfileDto?> GetUseProfileByUserIdAsync(int userId)
        {
            if (userId == null)
                return null;
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                return null;
            var userProfile = _mapper.Map<UserProfileDto>(user);
            return userProfile;
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        /// <summary>
        /// Get user by user code
        /// </summary>
        public async Task<UserDto?> GetUserByUserCodeAsync(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                return null;

            var user = await _unitOfWork.UserRepository.GetByUserCodeAsync(userCode);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<(bool Success, string Message, UserDto Data)> CreateUserAsync(UserDto userDto)
        {
            if (userDto == null)
                return (false, "User data cannot be null", null);

            if (string.IsNullOrWhiteSpace(userDto.Email))
                return (false, "Email is required", null);

            if (string.IsNullOrWhiteSpace(userDto.FullName))
                return (false, "Full name is required", null);

            if (string.IsNullOrWhiteSpace(userDto.UserCode))
                return (false, "User code is required", null);

            // Check if email already exists
            var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(userDto.Email);
            if (existingUser != null)
                return (false, "Email already exists", null);

            // Check if user code already exists
            var existingUserCode = await _unitOfWork.UserRepository.GetByUserCodeAsync(userDto.UserCode);
            if (existingUserCode != null)
                return (false, "User code already exists", null);

            var newUser = new User
            {
                UserCode = userDto.UserCode,
                FullName = userDto.FullName,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash),
                PhoneNumber = userDto.PhoneNumber,
                RoleId = userDto.RoleId,
                DepartmentId = userDto.DepartmentId,
                Status = userDto.Status ?? "ACTIVE",
            };
            newUser.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.UserRepository.CreateAsync(newUser);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            var createdUserDto = _mapper.Map<UserDto>(newUser);
            return (true, "User created successfully", createdUserDto);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateProfileUserAsync(int userId, UserUpdateProfileDto userDto)
        {
            if (userDto == null)
                return (false, "User data cannot be null");

            var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (existingUser == null)
                return (false, "User not found");

            // Check if email is being changed and if it already exists
            if (!string.IsNullOrWhiteSpace(userDto.Email) &&
                existingUser.Email != userDto.Email)
            {
                var emailExists = await _unitOfWork.UserRepository.EmailExistsAsync(userDto.Email);
                if (emailExists)
                    return (false, "Email already exists");

                existingUser.Email = userDto.Email;
            }

            // Update fields
            if (!string.IsNullOrWhiteSpace(userDto.FullName))
                existingUser.FullName = userDto.FullName;

            if (!string.IsNullOrWhiteSpace(userDto.PhoneNumber))
                existingUser.PhoneNumber = userDto.PhoneNumber;

            _unitOfWork.UserRepository.Update(existingUser);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "User updated successfully");
        }

        /// <summary>
        /// Delete a user (soft delete by marking as inactive)
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteUserAsync(string code)
        {
            var existingUser = await _unitOfWork.UserRepository.GetByUserCodeAsync(code);
            if (existingUser == null)
                return (false, "User not found");

            // Soft delete - set status to Inactive
            existingUser.Status = "Inactive";

            _unitOfWork.UserRepository.Update(existingUser);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "User deleted successfully");
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateUserAsync(string userCode, UserUpdateDto userDto)
        {
            if (userDto == null)
                return (false, "User data cannot be null");

            var existingUser = await _unitOfWork.UserRepository.GetByUserCodeAsync(userCode);
            if (existingUser == null)
                return (false, "User not found");

            // Update fields
            if (!string.IsNullOrWhiteSpace(userDto.FullName))
                existingUser.FullName = userDto.FullName;

            if (!string.IsNullOrWhiteSpace(userDto.PhoneNumber))
                existingUser.PhoneNumber = userDto.PhoneNumber;

            existingUser.RoleId = userDto.RoleId;
            existingUser.DepartmentId = userDto.DepartmentId;
            existingUser.Status = userDto.Status;

            _unitOfWork.UserRepository.Update(existingUser);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "User updated successfully");
        }

    }
}
