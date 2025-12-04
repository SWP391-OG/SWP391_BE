using SWP391.Contracts;
using SWP391.Contracts.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.CategoryService
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
