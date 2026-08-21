namespace HrMangmentSystem_Dto.DTOs.Department
{
    public class UpdateDepartmentDto
    {
    
        public int Id { get; set; }
        public int? ParentDepartmentId { get; set; }
        public string? Code { get; set; }
        public string? DeptName { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public Guid? DepartmentManagerId { get; set; }

    
        }

   
    
    }
