using AutoMapper;
using SWP391.Contracts;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using SWP391.Repositories.Repositories;
using SWP391.Services.CategoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var existingCode = await _unitOfWork.CategoryRepository.GetCatgoryByCodeAsync(dto.CategoryCode);
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

        public async Task<(bool Success, string Message)> DeleteCategoryByCodeAsync(string categoryCode)
        {
            var existingCode = await _unitOfWork.CategoryRepository.GetCatgoryByCodeAsync(categoryCode);
            if (existingCode == null)
                return (false, "Category code doesn't exists");
            await _unitOfWork.CategoryRepository.RemoveAsync(existingCode);
            return (true, "Category deleted successfully");
        }

        public async Task<List<CategoryDto>> GetAllCategoryAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            var exis = new List<CategoryDto>();
            CategoryDto existCategory;
            foreach (var category in categories)
            {
                existCategory = _mapper.Map<CategoryDto>(categories);
                exis.Add(existCategory);
            }
            return exis;
        }

        public async  Task<CategoryDto> GetByCategoryCodeAsync(string categoryCode)
        => await _unitOfWork.CategoryRepository.GetCatgoryByCodeAsync(categoryCode)
                .ContinueWith(task => _mapper.Map<CategoryDto>(task.Result));

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(CategoryRequestDto dto)
        {
            var category = await _unitOfWork.CategoryRepository.GetCatgoryByCodeAsync(dto.CategoryCode);
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

        public async Task<(bool Success, string Message)> UpdateStatusCategoryAsync(CategoryStatusUpdateDto dto)
        {
            var category = await _unitOfWork.CategoryRepository.GetCatgoryByCodeAsync(dto.CategoryCode);


            if (category == null)
            {
                return (false, "Category not found");
            }
            category.Status = dto.Status;
            _unitOfWork.CategoryRepository.Update(category);

            return (true, "Category status updated successfully");
        }
    }
}
