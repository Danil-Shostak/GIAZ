using EducationInstitutionsRB.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Services;

public class AppDbContext : DbContext
{
    public DbSet<Region> Regions { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Institution> Institutions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        try
        {
            var databasePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "education.db");
            var connectionString = $"Data Source={databasePath}";

            optionsBuilder.UseSqlite(connectionString);

            System.Diagnostics.Debug.WriteLine($"Database path: {databasePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error configuring database: {ex.Message}");
            throw;
        }
    }

    public void InitializeDatabase()
    {
        try
        {
            // Создаем базу данных и таблицы
            Database.EnsureCreated();
            System.Diagnostics.Debug.WriteLine("Database created successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating database: {ex.Message}");
            // Пробуем альтернативный метод
            CreateDatabaseManually();
        }
    }
    public async Task MigrateDatabaseAsync()
    {
        try
        {
            var databasePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "education.db");
            var connectionString = $"Data Source={databasePath}";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // Проверяем существование новых столбцов и добавляем их если нужно
            var columnsToAdd = new[]
            {
            "LicenseNumber", "LicenseExpiryDate", "AccreditationCategory", "OwnershipType",
            "LanguageOfEducation", "DirectorName", "Email", "Website", "FoundationYear",
            "InstitutionStatus", "ClassroomCount", "TeacherCount", "AdministrativeStaffCount",
            "ComputerCount", "HasSportsHall", "HasDiningRoom", "HasLibrary", "TotalArea",
            "Specialization", "EducationalPrograms", "Infrastructure"
        };

            foreach (var column in columnsToAdd)
            {
                try
                {
                    var checkCommand = connection.CreateCommand();
                    checkCommand.CommandText = $"PRAGMA table_info(Institutions);";

                    var reader = await checkCommand.ExecuteReaderAsync();
                    bool columnExists = false;

                    while (await reader.ReadAsync())
                    {
                        if (reader.GetString(1) == column)
                        {
                            columnExists = true;
                            break;
                        }
                    }
                    await reader.CloseAsync();

                    if (!columnExists)
                    {
                        var addCommand = connection.CreateCommand();

                        if (column == "LicenseExpiryDate")
                        {
                            addCommand.CommandText = $"ALTER TABLE Institutions ADD COLUMN {column} TEXT;";
                        }
                        else if (column == "HasSportsHall" || column == "HasDiningRoom" || column == "HasLibrary")
                        {
                            addCommand.CommandText = $"ALTER TABLE Institutions ADD COLUMN {column} INTEGER DEFAULT 0;";
                        }
                        else if (column == "TotalArea")
                        {
                            addCommand.CommandText = $"ALTER TABLE Institutions ADD COLUMN {column} REAL DEFAULT 0;";
                        }
                        else if (column == "FoundationYear" || column == "ClassroomCount" || column == "TeacherCount" ||
                                 column == "AdministrativeStaffCount" || column == "ComputerCount")
                        {
                            addCommand.CommandText = $"ALTER TABLE Institutions ADD COLUMN {column} INTEGER DEFAULT 0;";
                        }
                        else
                        {
                            addCommand.CommandText = $"ALTER TABLE Institutions ADD COLUMN {column} TEXT DEFAULT '';";
                        }

                        await addCommand.ExecuteNonQueryAsync();
                        Debug.WriteLine($"Added column: {column}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error adding column {column}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Migration error: {ex.Message}");
        }
    }

    private void CreateDatabaseManually()
    {
        try
        {
            var databasePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "education.db");
            var connectionString = $"Data Source={databasePath}";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Regions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Districts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                RegionId INTEGER NOT NULL,
                FOREIGN KEY (RegionId) REFERENCES Regions(Id)
            );

            CREATE TABLE IF NOT EXISTS Institutions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Type TEXT NOT NULL,
                Address TEXT NOT NULL,
                Contacts TEXT,
                DistrictId INTEGER NOT NULL,
                Status TEXT NOT NULL,
                RegistrationDate TEXT NOT NULL,
                StudentCount INTEGER NOT NULL DEFAULT 0,
                AdmittedCount INTEGER NOT NULL DEFAULT 0,
                ExpelledCount INTEGER NOT NULL DEFAULT 0,
                StaffCount INTEGER NOT NULL DEFAULT 0,
                
                -- НОВЫЕ ПОЛЯ
                LicenseNumber TEXT DEFAULT '',
                LicenseExpiryDate TEXT,
                AccreditationCategory TEXT DEFAULT '',
                OwnershipType TEXT DEFAULT 'Государственное',
                LanguageOfEducation TEXT DEFAULT 'Русский',
                DirectorName TEXT DEFAULT '',
                Email TEXT DEFAULT '',
                Website TEXT DEFAULT '',
                FoundationYear INTEGER DEFAULT 2024,
                InstitutionStatus TEXT DEFAULT 'Действующее',
                ClassroomCount INTEGER DEFAULT 0,
                TeacherCount INTEGER DEFAULT 0,
                AdministrativeStaffCount INTEGER DEFAULT 0,
                ComputerCount INTEGER DEFAULT 0,
                HasSportsHall INTEGER DEFAULT 0,
                HasDiningRoom INTEGER DEFAULT 0,
                HasLibrary INTEGER DEFAULT 0,
                TotalArea REAL DEFAULT 0,
                Specialization TEXT DEFAULT '',
                EducationalPrograms TEXT DEFAULT '',
                Infrastructure TEXT DEFAULT '',
                
                FOREIGN KEY (DistrictId) REFERENCES Districts(Id)
            );
        ";

            command.ExecuteNonQuery();
            System.Diagnostics.Debug.WriteLine("Database updated with extended fields");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating database: {ex.Message}");
        }
    }

    // Также добавляем в OnModelCreating:
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<District>()
            .HasOne(d => d.Region)
            .WithMany()
            .HasForeignKey(d => d.RegionId);

        modelBuilder.Entity<Institution>()
            .HasOne(i => i.District)
            .WithMany()
            .HasForeignKey(i => i.DistrictId);

        // Настройки по умолчанию для новых полей
        modelBuilder.Entity<Institution>(entity =>
        {
            entity.Property(i => i.LicenseNumber).HasDefaultValue("");
            entity.Property(i => i.AccreditationCategory).HasDefaultValue("");
            entity.Property(i => i.OwnershipType).HasDefaultValue("Государственное");
            entity.Property(i => i.LanguageOfEducation).HasDefaultValue("Русский");
            entity.Property(i => i.InstitutionStatus).HasDefaultValue("Действующее");
            entity.Property(i => i.FoundationYear).HasDefaultValue(DateTime.Now.Year);
        });
    }

    public void DetachAllEntities()
    {
        var changedEntriesCopy = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in changedEntriesCopy)
            entry.State = EntityState.Detached;
    }
}