using Microsoft.AspNetCore.Identity;

namespace EnglishC1.Client.Infrastructure.Identity;

// ASP.NET Core Identity's user type. Kept in Infrastructure, not Domain -
// it's tightly coupled to the Identity framework (password hash, security
// stamp, etc.), not a pure domain concept. Extend with app-specific
// profile fields here as they're needed (e.g. display name, CEFR level).
public class ApplicationUser : IdentityUser<Guid>
{
}
