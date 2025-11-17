using EducationInstitutionsRB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Services;

public interface IDataService
{
    // Regions
    Task<List<Region>> GetRegionsAsync();
    Task<Region?> GetRegionAsync(int id);
    Task AddRegionAsync(Region region);
    Task UpdateRegionAsync(Region region);
    Task DeleteRegionAsync(int id);

    // Districts
    Task<List<District>> GetDistrictsAsync();
    Task<List<District>> GetDistrictsByRegionAsync(int regionId);
    Task<District?> GetDistrictAsync(int id);
    Task AddDistrictAsync(District district);
    Task UpdateDistrictAsync(District district);
    Task DeleteDistrictAsync(int id);

    // Institutions
    Task<List<Institution>> GetInstitutionsAsync();
    Task<Institution?> GetInstitutionAsync(int id);
    Task AddInstitutionAsync(Institution institution);
    Task UpdateInstitutionAsync(Institution institution);
    Task DeleteInstitutionAsync(int id);

    // Search and Filter
    Task<List<Institution>> SearchInstitutionsAsync(string searchText, int? regionId, int? districtId, string? type, string? status);

    // Additional methods
    Task<List<string>> GetInstitutionTypesAsync();
    Task<List<string>> GetStatusTypesAsync();
    Task<string> GetDatabaseInfoAsync();

    void Dispose();
}