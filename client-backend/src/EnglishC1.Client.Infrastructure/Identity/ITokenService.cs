namespace EnglishC1.Client.Infrastructure.Identity;

public interface ITokenService
{
    string CreateToken(ApplicationUser user, IEnumerable<string> roles);
}
