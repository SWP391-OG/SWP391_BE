using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Contracts.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public int? SlaResolveHours { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class CategoryRequestDto
    {
        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public int? SlaResolveHours { get; set; }
    }

    public class CategoryStatusUpdateDto
    {
        public int CategoryId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
