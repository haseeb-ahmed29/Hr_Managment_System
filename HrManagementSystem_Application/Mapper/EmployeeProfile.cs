using AutoMapper;
using HrManagmentSystem_Shared.Enum.Employee;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Dto.DTOs.Employee;

namespace HrMangmentSystem_Application.Mapper
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            // Entity to DTO
            CreateMap<Employee, EmployeeDto>().
                ForMember(e => e.DepartmentName,
                opt => opt.MapFrom(src => src.Department.DeptName));

            // DTO to Entity
            CreateMap<CreateEmployeeDto, Employee>()
                .ForMember(e => e.EmploymentStatusType,
                opt => opt.MapFrom(_ => EmployeeStatus.Active))
                .ForMember(e => e.MustChangePassword,
                opt => opt.MapFrom(_ => true));

            // DTO to Entity for Update
            CreateMap<UpdateEmployeeDto, Employee>();
             
        }
    }
}
