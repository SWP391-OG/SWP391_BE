using SWP391.Repositories.Models;

namespace SWP391.Repositories.Interfaces
{
    public interface ILocationRepository
    {
        Task<Location?> GetLocationByCodeAsync(string code);
        Task<Location?> GetLocationByNameAsync(string name);
        Task<Location?> GetLocationByCodeWithCampusAsync(string code);
        Task<List<Location>> GetAllLocationsWithCampusAsync();
    }
}
