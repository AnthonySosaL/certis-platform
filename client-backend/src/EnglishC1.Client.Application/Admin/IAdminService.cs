namespace EnglishC1.Client.Application.Admin;

public interface IAdminService
{
    Task<List<StudentSummaryDto>> GetStudentSummariesAsync();
}
