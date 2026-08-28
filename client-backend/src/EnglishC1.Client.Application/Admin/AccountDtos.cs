namespace EnglishC1.Client.Application.Admin;

public record AccountDto(
    Guid UserId,
    string Email,
    bool IsAdmin,
    bool IsTutor,
    Guid? TutorId,
    string? TutorLabel);

public record SetRolesRequest(bool IsAdmin, bool IsTutor);

// TutorUserId null clears the assignment ("no tutor").
public record SetTutorRequest(Guid? TutorUserId);
