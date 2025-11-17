using System;

namespace EducationInstitutionsRB.Models;

public class Institution
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Contacts { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public District? District { get; set; }
    public string Status { get; set; } = "Активно";
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
    public int StudentCount { get; set; }
    public int AdmittedCount { get; set; }
    public int ExpelledCount { get; set; }
    public int StaffCount { get; set; }

    // Новые поля
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime? LicenseExpiryDate { get; set; }
    public string AccreditationCategory { get; set; } = string.Empty;
    public string OwnershipType { get; set; } = "Государственное";
    public string LanguageOfEducation { get; set; } = "Русский";
    public string DirectorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public int FoundationYear { get; set; } = DateTime.Now.Year;
    public string InstitutionStatus { get; set; } = "Действующее";
    public int ClassroomCount { get; set; }
    public int TeacherCount { get; set; }
    public int AdministrativeStaffCount { get; set; }
    public int ComputerCount { get; set; }
    public bool HasSportsHall { get; set; }
    public bool HasDiningRoom { get; set; }
    public bool HasLibrary { get; set; }
    public decimal TotalArea { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string EducationalPrograms { get; set; } = string.Empty;
    public string Infrastructure { get; set; } = string.Empty;

    // Вычисляемые свойства
    public string StudentCountDisplay => $"Учеников: {StudentCount}";
    public string StaffCountDisplay => $"Персонал: {StaffCount}";
    public string AdmittedCountDisplay => $"Принято: {AdmittedCount}";
    public string ExpelledCountDisplay => $"Отчислено: {ExpelledCount}";
    public string RegistrationDateDisplay => RegistrationDate.ToString("dd.MM.yyyy");
    public string LicenseExpiryDisplay => LicenseExpiryDate?.ToString("dd.MM.yyyy") ?? "Не указана";

    // ИСПРАВЛЕННОЕ вычисляемое свойство
    public string TeacherStudentRatio
    {
        get
        {
            if (TeacherCount > 0 && StudentCount > 0)
            {
                double ratio = (double)StudentCount / TeacherCount;
                return $"1:{ratio:F1}";
            }
            return "0";
        }
    }

    public override string ToString() => Name;
}