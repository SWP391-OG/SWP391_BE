using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Contracts.Location
{
    public class LocationDto
    {
        public int Id { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string CampusName { get; set; } = string.Empty;
        public string CampusCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class LocationRequestDto
    {
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public int CampusId { get; set; } 
    }

    public class LocationStatusUpdateDto
    {
        public int LocationId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
