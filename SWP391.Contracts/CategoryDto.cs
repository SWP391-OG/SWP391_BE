using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Contracts
{
    public class CategoryDto
    {
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
        public string CategoryCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
