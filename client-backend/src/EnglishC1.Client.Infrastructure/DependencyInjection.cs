using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using EnglishC1.Client.Application.Admin;
using EnglishC1.Client.Application.Ai;
using EnglishC1.Client.Application.PlacementTest;
using EnglishC1.Client.Infrastructure.Admin;
using EnglishC1.Client.Infrastructure.Ai;
using EnglishC1.Client.Infrastructure.Identity;
using EnglishC1.Client.Infrastructure.Persistence;
using EnglishC1.Client.Infrastructure.PlacementTest;

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
                // Chosen policy: 8+ characters, at least one digit. Nothing
                // stricter (no forced uppercase/symbol) - simple enough to
                // state in one line of UI hint text. This exact rule must
                // stay in sync with the Angular Register form's validators
                // (register.ts) - a previous mismatch between the two
                // caused a real password to get rejected with a message
                // that looked like "email already taken"; see
                // docs/errors/2026-08-27-password-policy-mismatch.md.
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
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
        services.Configure<GroqOptions>(configuration.GetSection(GroqOptions.SectionName));
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ITestService, TestService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddHttpClient<IAiInsightService, GroqInsightService>(client => client.Timeout = TimeSpan.FromSeconds(20));
        services.AddHttpClient<IAiQuestionGeneratorService, GroqQuestionGeneratorService>(client => client.Timeout = TimeSpan.FromSeconds(20));

        return services;
    }
}
