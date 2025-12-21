using AutoMapper;
using SWP391.Contracts.Category;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;


namespace SWP391.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        /// <summary>
        /// Service for managing category operations
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<(bool Success, string Message, CategoryDto Data)> CreateCategoryAsync(CategoryRequestDto dto)
        {
            if (dto == null)
                return (false, "Category data cannot be null", null);

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
                return (false, "Invalid category ID");
            
            var existingCode = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (existingCode == null)
                return (false, "Category code doesn't exists");

           existingCode.Status = "INACTIVE"; // Soft delete by setting status to Inactive
            await _unitOfWork.CategoryRepository.UpdateAsync(existingCode);
            return (true, "Category deleted successfully");
        }
       
        /// <summary>
        /// Get all active categories
        /// </summary>
        public async Task<List<CategoryDto>> GetAllActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllActiveCategoriesAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        /// <summary>
        /// Get all categories (active and inactive)
        /// </summary>
        public async Task<List<CategoryDto>> GetAllCategoryAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        /// <summary>
        /// Get category by category code
        /// </summary>
        public async Task<CategoryDto> GetByCategoryCodeAsync(string categoryCode)
        {
            if (string.IsNullOrWhiteSpace(categoryCode))
                return null;

            var category = await _unitOfWork.CategoryRepository.GetCategoryByCodeAsync(categoryCode);
            return category == null ? null : _mapper.Map<CategoryDto>(category);
        }

        
        /// <summary>
        /// Update category information
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateCategoryAsync(int categoryId, CategoryRequestDto dto)
        {
            if (categoryId <= 0)
                return (false, "Invalid category ID");

            if (dto == null)
                return (false, "Category data cannot be null");

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null)            
                return (false, "Category not found");
            
             // Update fields
            if (!string.IsNullOrWhiteSpace(dto.CategoryCode))
                category.CategoryCode = dto.CategoryCode;

            if (!string.IsNullOrWhiteSpace(dto.CategoryName))
                category.CategoryName = dto.CategoryName;

            if (dto.DepartmentId > 0)
                category.DepartmentId = dto.DepartmentId;

            if (dto.SlaResolveHours > 0)
                category.SlaResolveHours = dto.SlaResolveHours;

            await _unitOfWork.CategoryRepository.UpdateAsync(category);
            return (true, "Category updated successfully");
        }

        /// <summary>
        /// Update category status
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateStatusCategoryAsync(CategoryStatusUpdateDto dto)
        {
            if (dto == null)
                return (false, "Status update request cannot be null");

            if(dto.CategoryId <= 0)           
                return (false, "Invalid category ID");
                
            if (string.IsNullOrWhiteSpace(dto.Status))
                return (false, "Status cannot be empty");    
            
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(dto.CategoryId);
   
            if (category == null)           
                return (false, "Category not found");
            
            category.Status = dto.Status;
            await _unitOfWork.CategoryRepository.UpdateAsync(category);

            return (true, "Category status updated successfully");
        }


    }
}
