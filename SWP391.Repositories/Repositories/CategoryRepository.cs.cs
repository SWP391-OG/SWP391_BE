using Microsoft.EntityFrameworkCore;
using SWP391.Repositories.Basic;
using SWP391.Repositories.DBContext;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Repositories.Repositories
{
    public class CategoryRepository : GenericRepository<Category> , ICategoryRepository
    {
        public CategoryRepository() => _context ??= new FPTechnicalContext();

        public CategoryRepository(FPTechnicalContext context) => _context = context;

        public async Task<Category?> GetCatgoryByCodeAsync(string code)
        {
            var location = await _context.Categories.FirstOrDefaultAsync(d => d.CategoryCode == code);
            return location;
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            var location = await _context.Categories.FirstOrDefaultAsync(d => d.CategoryName == name);
            return location;
        }
    }
  
}
