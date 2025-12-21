using AutoMapper;
using SWP391.Contracts.User;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;

namespace SWP391.Services.UserServices
{
    /// <summary>
    /// Service for managing user operations
    /// </summary>
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
        /// Get all users with their department information
        /// </summary>
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllUsersWithDepartment();
            return _mapper.Map<List<UserDto>>(users);          
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
           return user == null ? null : _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Get profile by user Id
        /// </summary>
        public async Task<UserProfileDto?> GetUseProfileByUserIdAsync(int userId)
        {
           if (userId <= 0)
                return null;

           var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            return user == null ? null : _mapper.Map<UserProfileDto>(user);
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Get user by user code
        /// </summary>
        public async Task<UserDto?> GetUserByUserCodeAsync(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                return null;

            var user = await _unitOfWork.UserRepository.GetByUserCodeAsync(userCode);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }       

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<(bool Success, string Message, UserDto Data)> CreateUserAsync(UserCreateDto userDto)
        {
              // Validate input
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

            // Create new user
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
                CreatedAt = DateTime.UtcNow
            };
           
            await _unitOfWork.UserRepository.CreateAsync(newUser);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            var createdUserDto = _mapper.Map<UserDto>(newUser);
            return (true, "User created successfully", createdUserDto);
        }

        /// <summary>
        /// Update an existing user  (name, email, phone only)
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateProfileUserAsync(int userId, UserUpdateProfileDto userDto)
        {
            if (userDto == null)
                return (false, "User data cannot be null");

            if (userId <= 0)
                return (false, "Invalid user ID");

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

            await _unitOfWork.UserRepository.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "User updated successfully");
        }

        /// <summary>
        /// Delete a user (soft delete by marking as inactive)
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            if(userId < 0)
                return (false, "Invalid user ID");

            var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (existingUser == null)
                return (false, "User not found");

            existingUser.Status = "INACTIVE";
            await _unitOfWork.UserRepository.UpdateAsync(existingUser);     
            return (true, "User deleted successfully");
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateUserAsync(int userId, UserUpdateDto userDto)
        {
            if (userDto == null)
                return (false, "User data cannot be null");

             if (userId <= 0)
                return (false, "Invalid user ID");

            var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (existingUser == null)
                return (false, "User not found");

            // Update fields
            if (!string.IsNullOrWhiteSpace(userDto.FullName))
                existingUser.FullName = userDto.FullName;

            if (!string.IsNullOrWhiteSpace(userDto.PhoneNumber))
                existingUser.PhoneNumber = userDto.PhoneNumber;

            if (userDto.RoleId > 0)
                existingUser.RoleId = userDto.RoleId;

            if(!string.IsNullOrWhiteSpace(userDto.UserCode))
                existingUser.UserCode = userDto.UserCode;

            if (userDto.DepartmentId > 0)
                existingUser.DepartmentId = userDto.DepartmentId;

            if (!string.IsNullOrWhiteSpace(userDto.Email) && existingUser.Email != userDto.Email)
            {
                if (await _unitOfWork.UserRepository.EmailExistsAsync(userDto.Email))
                    return (false, "Email already exists");
                existingUser.Email = userDto.Email;
            }

            if (!string.IsNullOrWhiteSpace(userDto.PasswordHash))
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash);

            await _unitOfWork.UserRepository.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "User updated successfully");
        }

        /// <summary>
        /// Update user status
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateUserStatusAsync(UserStatusUpdateDto dto)
        {
            if (dto == null)
                return (false, "Status update request cannot be null");

           if(dto.UserId <= 0)
                return (false, "Invalid user ID");

            if (string.IsNullOrWhiteSpace(dto.Status))
                return (false, "Status cannot be empty");

            var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId);
            if (existingUser == null)
                return (false, "User not found");

            existingUser.Status = dto.Status;
            await _unitOfWork.UserRepository.UpdateAsync(existingUser);
            return (true, "User updated successfully");
        }
    }
}
