using SWP391.Contracts.Category;
using SWP391.Repositories.Models;

namespace SWP391.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoryAsync();
        Task<CategoryDto> GetByCategoryCodeAsync(string categoryCode);
        Task<(bool Success, string Message, CategoryDto Data)> CreateCategoryAsync(CategoryRequestDto dto);
        Task<(bool Success, string Message)> UpdateCategoryAsync(int categpryId ,CategoryRequestDto dto);
        Task<(bool Success, string Message)> UpdateStatusCategoryAsync(int categoryId);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int locationId);
        Task<List<CategoryDto>> GetAllActiveCategoriesAsync();
    }
}
