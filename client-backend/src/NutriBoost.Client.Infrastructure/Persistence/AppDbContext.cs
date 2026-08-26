using Microsoft.EntityFrameworkCore;

namespace NutriBoost.Client.Infrastructure.Persistence;

// No entities yet — the real domain model (exercises, attempts, users,
// progress...) depends on the actual feature scope of the English-practice
// platform, still being defined. See docs/PENDING_IDEAS.md.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
