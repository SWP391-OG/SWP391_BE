using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Contracts.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string DeptCode { get; set; } = string.Empty;

        public string DeptName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

    }

    public class DepartmentRequestDto
    {
        public string DeptCode { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;
    }

    public class DepartmentStatusUpdateDto
    {
        public string DeptCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
