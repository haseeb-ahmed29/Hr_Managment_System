namespace HrMangmentSystem_Infrastructure.Interfaces.Repository
{
    public interface ICurrentUser
    {
     

        Guid EmployeeId { get; }

        string? Email { get; }

        Guid? TenantId { get; }

        IReadOnlyList<string> Roles { get; }

    }
}
