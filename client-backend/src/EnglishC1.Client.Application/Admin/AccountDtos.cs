namespace EnglishC1.Client.Application.Admin;

public record AccountDto(Guid UserId, string Email, bool IsAdmin, bool IsTutor);

public record SetRolesRequest(bool IsAdmin, bool IsTutor);
