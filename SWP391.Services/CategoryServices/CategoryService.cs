using AutoMapper;
using SWP391.Contracts.Category;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;


namespace SWP391.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<(bool Success, string Message, CategoryDto Data)> CreateCategoryAsync(CategoryRequestDto dto)
        {
            var existingCode = await _unitOfWork.CategoryRepository.GetCategoryByCodeAsync(dto.CategoryCode);
            if (existingCode != null)
                return (false, "Category code already exists", null);

            var existingName = await _unitOfWork.CategoryRepository.GetCategoryByNameAsync(dto.CategoryName);
            if (existingName != null)
                return (false, "Category name already exists", null);

            var category = _mapper.Map<Category>(dto);
            await _unitOfWork.CategoryRepository.CreateAsync(category);
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return (true, "Category created successfully", categoryDto);
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int categoryId)
        {
            if(categoryId <= 0)
            {
                return (false, "Invalid category ID");
            }

            var existingCode = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (existingCode == null)
                return (false, "Category code doesn't exists");

           existingCode.Status = "INACTIVE"; // Soft delete by setting status to Inactive
            await _unitOfWork.CategoryRepository.UpdateAsync(existingCode);
            return (true, "Category deleted successfully");
        }

        public async Task<List<CategoryDto>> GetAllActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllActiveCategoriesAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<List<CategoryDto>> GetAllCategoryAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetByCategoryCodeAsync(string categoryCode)
        => await _unitOfWork.CategoryRepository.GetCategoryByCodeAsync(categoryCode)
                .ContinueWith(task => _mapper.Map<CategoryDto>(task.Result));

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(int categoryId, CategoryRequestDto dto)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                return (false, "Category not found");
            }
            category.CategoryCode = dto.CategoryCode;
            category.CategoryName = dto.CategoryName;
            category.DepartmentId = dto.DepartmentId;
            category.SlaResolveHours = dto.SlaResolveHours;

            _unitOfWork.CategoryRepository.Update(category);

            return (true, "Category updated successfully");
        }

        public async Task<(bool Success, string Message)> UpdateStatusCategoryAsync(int categoryId)
        {
            if(categoryId <= 0)
            {
                return (false, "Invalid category ID");
            }
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
   
            if (category == null)
            {
                return (false, "Category not found");
            }

            if(category.Status == "ACTIVE")
            {
                category.Status = "INACTIVE";
            }
            else
            {
                category.Status = "ACTIVE";
            }
            
            _unitOfWork.CategoryRepository.Update(category);

            return (true, "Category status updated successfully");
        }


    }
}
