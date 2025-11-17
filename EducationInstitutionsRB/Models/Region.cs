namespace EducationInstitutionsRB.Models;

public class Region
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}