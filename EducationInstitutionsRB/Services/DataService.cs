using EducationInstitutionsRB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Services;

public class DataService : IDataService
{
    private readonly AppDbContext _context;

    public DataService()
    {
        _context = new AppDbContext();
        _context.InitializeDatabase();

        // Добавляем миграцию
        _ = MigrateDatabaseAsync();
        InitializeSampleData();
    }

    private async Task MigrateDatabaseAsync()
    {
        try
        {
            await _context.MigrateDatabaseAsync();
            Debug.WriteLine("Database migration completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Database migration error: {ex.Message}");
        }
    }

    private async void InitializeSampleData()
    {
        try
        {
            // Проверяем, есть ли уже данные
            if (!await _context.Regions.AnyAsync())
            {
                System.Diagnostics.Debug.WriteLine("Создание тестовых данных...");

                var region1 = new Region { Name = "Минская область" };
                var region2 = new Region { Name = "Гродненская область" };
                var region3 = new Region { Name = "Брестская область" };

                _context.Regions.AddRange(region1, region2, region3);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Регионы созданы");

                var district1 = new District { Name = "Минский район", RegionId = region1.Id };
                var district2 = new District { Name = "Гродненский район", RegionId = region2.Id };
                var district3 = new District { Name = "Брестский район", RegionId = region3.Id };

                _context.Districts.AddRange(district1, district2, district3);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Районы созданы");

                var institution1 = new Institution
                {
                    Name = "Гимназия №1 г. Минска",
                    Type = "Гимназия",
                    Address = "г. Минск, ул. Примерная, 1",
                    Contacts = "+375-17-123-45-67",
                    DistrictId = district1.Id,
                    Status = "Активно",
                    RegistrationDate = new DateTime(2020, 1, 15),
                    StudentCount = 850,
                    AdmittedCount = 120,
                    ExpelledCount = 15,
                    StaffCount = 65,
                    // ДОБАВЛЯЕМ НОВЫЕ ПОЛЯ
                    TeacherCount = 45, // Преподаватели
                    AdministrativeStaffCount = 20, // Административный персонал
                    ClassroomCount = 30,
                    ComputerCount = 50,
                    HasSportsHall = true,
                    HasDiningRoom = true,
                    HasLibrary = true,
                    TotalArea = 2500.5m,
                    LicenseNumber = "12345-Л",
                    DirectorName = "Иванова Мария Петровна",
                    Email = "gym1@edu.by",
                    Website = "https://gym1.edu.by",
                    FoundationYear = 1990,
                    AccreditationCategory = "I категория"
                };

                var institution2 = new Institution
                {
                    Name = "Средняя школа №2 г. Гродно",
                    Type = "Школа",
                    Address = "г. Гродно, ул. Школьная, 10",
                    Contacts = "+375-152-345-678",
                    DistrictId = district2.Id,
                    Status = "Активно",
                    RegistrationDate = new DateTime(2018, 9, 1),
                    StudentCount = 620,
                    AdmittedCount = 85,
                    ExpelledCount = 8,
                    StaffCount = 45,
                    // ДОБАВЛЯЕМ НОВЫЕ ПОЛЯ
                    TeacherCount = 38, // Преподаватели
                    AdministrativeStaffCount = 7, // Административный персонал
                    ClassroomCount = 25,
                    ComputerCount = 35,
                    HasSportsHall = true,
                    HasDiningRoom = false,
                    HasLibrary = true,
                    TotalArea = 1800.0m,
                    LicenseNumber = "67890-Л",
                    DirectorName = "Петров Алексей Иванович",
                    Email = "school2@grodno.by",
                    Website = "https://school2.edu.by",
                    FoundationYear = 1985,
                    AccreditationCategory = "II категория"
                };

                _context.Institutions.AddRange(institution1, institution2);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("Тестовые данные успешно созданы с новыми полями");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Тестовые данные уже существуют");

                // Выводим отладочную информацию о существующих данных
                var regionsCount = await _context.Regions.CountAsync();
                var districtsCount = await _context.Districts.CountAsync();
                var institutionsCount = await _context.Institutions.CountAsync();

                System.Diagnostics.Debug.WriteLine($"Регионов: {regionsCount}, Районов: {districtsCount}, Учреждений: {institutionsCount}");

                // Проверяем данные учреждений
                var institutions = await _context.Institutions.ToListAsync();
                foreach (var institution in institutions)
                {
                    System.Diagnostics.Debug.WriteLine($"Учреждение: {institution.Name}, TeacherCount: {institution.TeacherCount}, StudentCount: {institution.StudentCount}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при создании тестовых данных: {ex.Message}");
        }
    }

    // Regions CRUD
    public async Task<List<Region>> GetRegionsAsync()
    {
        return await _context.Regions.ToListAsync();
    }

    public async Task<Region?> GetRegionAsync(int id)
    {
        return await _context.Regions.FindAsync(id);
    }

    public async Task AddRegionAsync(Region region)
    {
        _context.Regions.Add(region);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRegionAsync(Region region)
    {
        var existing = await _context.Regions.FindAsync(region.Id);
        if (existing != null)
        {
            existing.Name = region.Name;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteRegionAsync(int id)
    {
        var region = await _context.Regions.FindAsync(id);
        if (region != null)
        {
            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();
        }
    }

    // Districts CRUD
    public async Task<List<District>> GetDistrictsAsync()
    {
        return await _context.Districts.Include(d => d.Region).ToListAsync();
    }

    public async Task<List<District>> GetDistrictsByRegionAsync(int regionId)
    {
        return await _context.Districts
            .Where(d => d.RegionId == regionId)
            .Include(d => d.Region)
            .ToListAsync();
    }

    public async Task<District?> GetDistrictAsync(int id)
    {
        return await _context.Districts
            .Include(d => d.Region)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task AddDistrictAsync(District district)
    {
        _context.Districts.Add(district);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDistrictAsync(District district)
    {
        var existing = await _context.Districts.FindAsync(district.Id);
        if (existing != null)
        {
            existing.Name = district.Name;
            existing.RegionId = district.RegionId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteDistrictAsync(int id)
    {
        var district = await _context.Districts.FindAsync(id);
        if (district != null)
        {
            _context.Districts.Remove(district);
            await _context.SaveChangesAsync();
        }
    }

    // Institutions CRUD
    public async Task<List<Institution>> GetInstitutionsAsync()
    {
        return await _context.Institutions
            .Include(i => i.District)
                .ThenInclude(d => d.Region)
            .ToListAsync();
    }

    public async Task<Institution?> GetInstitutionAsync(int id)
    {
        return await _context.Institutions
            .Include(i => i.District)
                .ThenInclude(d => d.Region)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task AddInstitutionAsync(Institution institution)
    {
        _context.Institutions.Add(institution);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateInstitutionAsync(Institution institution)
    {
        try
        {
            // Способ 1: Поиск и обновление существующей записи
            var existing = await _context.Institutions
                .FirstOrDefaultAsync(i => i.Id == institution.Id);

            if (existing != null)
            {
                // Обновляем все свойства
                existing.Name = institution.Name;
                existing.Type = institution.Type;
                existing.Address = institution.Address;
                existing.Contacts = institution.Contacts;
                existing.DistrictId = institution.DistrictId;
                existing.Status = institution.Status;
                existing.RegistrationDate = institution.RegistrationDate;
                existing.StudentCount = institution.StudentCount;
                existing.AdmittedCount = institution.AdmittedCount;
                existing.ExpelledCount = institution.ExpelledCount;
                existing.StaffCount = institution.StaffCount;

                await _context.SaveChangesAsync();
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка параллелизма при обновлении: {ex.Message}");
            // Если возникла ошибка параллелизма, используем альтернативный способ
            await UpdateInstitutionAlternativeAsync(institution);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Общая ошибка при обновлении: {ex.Message}");
            throw;
        }
    }

    private async Task UpdateInstitutionAlternativeAsync(Institution institution)
    {
        // Альтернативный способ: используем новый контекст
        using var newContext = new AppDbContext();

        var existing = await newContext.Institutions
            .FirstOrDefaultAsync(i => i.Id == institution.Id);

        if (existing != null)
        {
            // Копируем все свойства
            existing.Name = institution.Name;
            existing.Type = institution.Type;
            existing.Address = institution.Address;
            existing.Contacts = institution.Contacts;
            existing.DistrictId = institution.DistrictId;
            existing.Status = institution.Status;
            existing.RegistrationDate = institution.RegistrationDate;
            existing.StudentCount = institution.StudentCount;
            existing.AdmittedCount = institution.AdmittedCount;
            existing.ExpelledCount = institution.ExpelledCount;
            existing.StaffCount = institution.StaffCount;

            await newContext.SaveChangesAsync();
        }
    }

    public async Task DeleteInstitutionAsync(int id)
    {
        var institution = await _context.Institutions.FindAsync(id);
        if (institution != null)
        {
            _context.Institutions.Remove(institution);
            await _context.SaveChangesAsync();
        }
    }

    // Search and Filter
    public async Task<List<Institution>> SearchInstitutionsAsync(string searchText, int? regionId, int? districtId, string? type, string? status)
    {
        var query = _context.Institutions
            .Include(i => i.District)
                .ThenInclude(d => d.Region)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(i => i.Name.Contains(searchText));
        }

        if (regionId.HasValue)
        {
            query = query.Where(i => i.District.RegionId == regionId.Value);
        }

        if (districtId.HasValue)
        {
            query = query.Where(i => i.DistrictId == districtId.Value);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(i => i.Type == type);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        return await query.ToListAsync();
    }

    // Additional methods for getting types and statuses
    public async Task<List<string>> GetInstitutionTypesAsync()
    {
        await Task.Delay(50); // Имитация асинхронной операции
        return new List<string> { "Школа", "Гимназия", "Лицей", "Колледж", "Университет" };
    }

    public async Task<List<string>> GetStatusTypesAsync()
    {
        await Task.Delay(50); // Имитация асинхронной операции
        return new List<string> { "Активно", "Закрыто", "На реконструкции" };
    }

    // Метод для проверки существования данных
    public async Task<string> GetDatabaseInfoAsync()
    {
        try
        {
            var regionsCount = await _context.Regions.CountAsync();
            var districtsCount = await _context.Districts.CountAsync();
            var institutionsCount = await _context.Institutions.CountAsync();

            return $"Регионов: {regionsCount}, Районов: {districtsCount}, Учреждений: {institutionsCount}";
        }
        catch (Exception ex)
        {
            return $"Ошибка получения информации: {ex.Message}";
        }
    }

    // Dispose pattern
    public void Dispose()
    {
        _context?.Dispose();
    }
}