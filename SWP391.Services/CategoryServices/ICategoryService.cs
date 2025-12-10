using SWP391.Contracts.Category;

namespace SWP391.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoryAsync();
        Task<CategoryDto> GetByCategoryCodeAsync(string categoryCode);
        Task<(bool Success, string Message, CategoryDto Data)> CreateCategoryAsync(CategoryRequestDto dto);
        Task<(bool Success, string Message)> UpdateCategoryAsync(CategoryRequestDto dto);
        Task<(bool Success, string Message)> UpdateStatusCategoryAsync(CategoryStatusUpdateDto dto);
        Task<(bool Success, string Message)> DeleteCategoryByCodeAsync(string locationCode);
    }
}
