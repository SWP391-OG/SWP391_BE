using Microsoft.EntityFrameworkCore;
using SWP391.Repositories.Basic;
using SWP391.Repositories.DBContext;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;

namespace SWP391.Repositories.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository() => _context ??= new FPTechnicalContext();

        public UserRepository(FPTechnicalContext context) => _context = context;

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>()
                .Include(u => u.Role)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Set<User>().AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUserCodeAsync(string userCode)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.UserCode == userCode);
        }

        public async Task<List<User>> GetAllUsersWithDepartment()
        {
            return await _context.Set<User>()
                .Include(u => u.Department)
                .ToListAsync();
        }
    }
}
