using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Repositories.Interfaces
{
    public interface ICampusRepository
    {
        Task<Campus?> GetCampusByCodeAsync(string code);
        Task<List<Location>> GetLocationByCampusCodeAsync(string code);
    }   
}
