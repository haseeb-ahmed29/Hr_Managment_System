
using HrManagmentSystem_Shared.Enum.Roles;

namespace HrMangmentSystem_Domain.Constants
{
    public static class RoleNames
    {
        public const string SystemAdmin = nameof(RoleType.SystemAdmin);
        public const string HrAdmin = nameof(RoleType.HrAdmin);
        public const string Manager = nameof(RoleType.Manager);
        public const string Employee = nameof(RoleType.Employee);
        public const string Recruiter = nameof(RoleType.Recruiter);
    }
}
