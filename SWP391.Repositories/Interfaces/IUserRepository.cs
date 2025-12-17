using SWP391.Repositories.Models;

namespace SWP391.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByUserCodeAsync(string userCode);
        Task<List<User>> GetAllUsersWithDepartment();

        // Inherited from GenericRepository (make sure these are available):
        // Task<User?> GetByIdAsync(int id);
        // Task<int> CreateAsync(User entity);
        // Task<int> UpdateAsync(User entity);
    }
}
