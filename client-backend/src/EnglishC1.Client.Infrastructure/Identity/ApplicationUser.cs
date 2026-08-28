using Microsoft.AspNetCore.Identity;

namespace EnglishC1.Client.Infrastructure.Identity;

// ASP.NET Core Identity's user type. Kept in Infrastructure, not Domain -
// it's tightly coupled to the Identity framework (password hash, security
// stamp, etc.), not a pure domain concept. Extend with app-specific
// profile fields here as they're needed (e.g. display name, CEFR level).
public class ApplicationUser : IdentityUser<Guid>
{
    // Shown instead of Email wherever a friendlier label makes sense -
    // e.g. the seeded "AI Tutor" persona (see Program.cs) needs a name a
    // student recognizes, not a throwaway local-only email address.
    public string? DisplayName { get; set; }

    // Self-referencing, optional - a student's currently assigned tutor
    // (2026-08-28, "tutor-student assignment"). One tutor per student at
    // a time by design (not a many-to-many junction table) - simplest
    // model that satisfies "Your tutor: X" without over-building before
    // there's a real need for co-tutoring.
    public Guid? TutorId { get; set; }
    public ApplicationUser? Tutor { get; set; }
}
