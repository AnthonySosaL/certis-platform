using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using EnglishC1.Client.Infrastructure.Identity;
using EnglishC1.Client.Infrastructure.Persistence;

namespace EnglishC1.Client.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AppDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:AppDb.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Identity's real defaults (uppercase + lowercase + digit +
                // non-alphanumeric, all required) are stricter than what
                // the frontend actually validates (8-char minimum only) -
                // that mismatch is what caused a real password to get
                // rejected with a message that looked like "email already
                // taken" (see docs/errors/2026-08-27-password-policy-mismatch.md).
                // Length-only here so backend and frontend agree.
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // no email sender wired up yet - see docs/PENDING_IDEAS.md
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();
        // AddDefaultTokenProviders() intentionally not called yet - it's
        // only needed for password-reset/email-confirmation tokens, which
        // aren't built yet (no email sender configured - see
        // docs/PENDING_IDEAS.md). Add it back when that's built.

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
