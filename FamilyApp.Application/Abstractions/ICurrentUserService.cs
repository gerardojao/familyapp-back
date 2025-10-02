namespace FamilyApp.Application.Abstractions;

public interface ICurrentUserService
{
    string? UserIdOrEmail { get; }
    bool IsAuthenticated { get; }
    int? UserIdInt { get; }
}
