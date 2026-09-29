using Dawaya.Api.Domain.Profiles;
using Microsoft.AspNetCore.Identity;

namespace Dawaya.Api.Domain.Identity;

// Roles (User, Admin, Pharmacist) use ASP.NET Identity roles, not a column.
public class ApplicationUser : IdentityUser
{
    public string PreferredLanguage { get; set; } = "ar";
    public bool LargeText { get; set; }
    public bool HighContrast { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<PatientProfile> Profiles { get; set; } = new List<PatientProfile>();
}
