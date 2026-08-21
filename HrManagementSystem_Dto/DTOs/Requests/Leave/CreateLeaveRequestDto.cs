using HrManagmentSystem_Shared.Enum.Request;

namespace HrMangmentSystem_Dto.DTOs.Requests.Leave
{
    public class CreateLeaveRequestDto
    {
        public LeaveType LeaveType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }
    }
}
