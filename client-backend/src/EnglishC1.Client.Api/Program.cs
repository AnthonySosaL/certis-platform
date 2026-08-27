using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EnglishC1.Client.Infrastructure;
using EnglishC1.Client.Infrastructure.Identity;
using EnglishC1.Client.Infrastructure.Persistence;
using EnglishC1.Client.Infrastructure.PlacementTest;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

// String enums (not the default numeric 0/1/2) - the placement test API
// sends CefrLevel/SkillArea/AttemptKind to the Angular client, which
// expects readable string literals ("B1", "Grammar"), not magic numbers.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Lets Swagger UI send "Authorization: Bearer <token>" for endpoints
    // that need it - paste the token from /api/auth/login's response.
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = [] });
});

builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing Jwt configuration section.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
        };
    });
builder.Services.AddAuthorization();

// Local dev origin (Angular's ng serve default) plus the deployed
// frontend origin (english-c1.runasp.net, live since 2026-08-27). Both
// http and https allowed for now since the frontend site's HTTPS
// certificate hadn't finished provisioning yet at launch - drop the
// http entry once it's confirmed HTTPS-only.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200", "http://english-c1.runasp.net", "https://english-c1.runasp.net")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Seeds the placement test question bank if empty. Requires the
// PlacementTest migration to already be applied (`dotnet ef database
// update`) - a no-op otherwise since the table won't exist yet.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if ((await db.Database.GetAppliedMigrationsAsync()).Any())
        await QuestionSeeder.SeedAsync(db);
}

// Ensures the "Admin" role exists and grants it to whichever account is
// configured as Admin:Email (a user-secret locally, an app setting in
// production) - idempotent, safe to run every startup. There's no admin
// UI to grant this role from; it's config-only on purpose, since letting
// anyone self-select "sign in as admin" at login (as a literal reading of
// the original request would do) is a real privilege-escalation footgun.
// The account itself decides admin status, not the login form.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
    if (!await roleManager.RoleExistsAsync("Tutor"))
        await roleManager.CreateAsync(new IdentityRole<Guid>("Tutor"));

    var adminEmail = builder.Configuration["Admin:Email"];
    if (!string.IsNullOrWhiteSpace(adminEmail))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();
