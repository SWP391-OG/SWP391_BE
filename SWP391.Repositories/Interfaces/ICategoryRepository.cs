using SWP391.Repositories.Models;

namespace SWP391.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetCategoryByCodeAsync(string code);
        Task<Category?> GetCategoryByNameAsync(string name);
    }
}
