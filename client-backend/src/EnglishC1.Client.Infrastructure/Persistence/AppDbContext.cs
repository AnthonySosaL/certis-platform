using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EnglishC1.Client.Infrastructure.Identity;

namespace EnglishC1.Client.Infrastructure.Persistence;

// IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> brings in
// the Identity tables (AspNetUsers, AspNetRoles, etc.) - that's the
// domain model so far. The rest (exercises, attempts, progress...) still
// depends on the feature scope being defined; see docs/PENDING_IDEAS.md.
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
