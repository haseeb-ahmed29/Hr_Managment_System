using System.ComponentModel.DataAnnotations;

namespace HrMangmentSystem_Dto.DTOs.Department
{
    public class CreateDepartmentDto
    {
        [Required]
        public string Code { get; set; } = null!;
        [Required]
        public string DeptName { get; set; } = null!;
       
        public string Description { get; set; } = null!;
       
        public string Location { get; set; } = null!;

        public int? ParentDepartmentId { get; set; }

        
        public Guid? DepartmentManagerId { get; set; }




    }
}
