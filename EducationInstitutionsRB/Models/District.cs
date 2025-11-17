namespace EducationInstitutionsRB.Models;

public class District
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RegionId { get; set; }
    public Region? Region { get; set; }

    public override string ToString() => Name;
}