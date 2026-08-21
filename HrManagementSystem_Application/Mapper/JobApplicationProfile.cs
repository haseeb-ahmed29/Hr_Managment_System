using AutoMapper;
using HrMangmentSystem_Domain.Entities.Recruitment;
using HrMangmentSystem_Dto.DTOs.Job.Appilcation;

namespace HrMangmentSystem_Application.Mapper
{
    public class JobApplicationProfile : Profile
    {
        public JobApplicationProfile()
        {
            CreateMap<JobApplication, JobApplicationDto>()
                .ForMember(ja => ja.JobPositionTitle,
                               opt => opt.MapFrom(src => src.JobPosition.Title))
                .ForMember(ja => ja.ReviewedByEmployeeName,
                               opt => opt.MapFrom(src =>
                                         src.ReviewedByEmployeeId != null ?
                                         src.ReviewedByEmployee.FirstName + " " + src.ReviewedByEmployee.LastName
                                         : null));

            //CreateMap<CreateJobApplicationDto, JobApplicationDto>()
            //    .ForMember(ja => ja.Status,
            //    opt => opt.MapFrom(_ => JobApplicationStatus.New))
            //    .ForMember(ja => ja.AppliedAt,
            //    opt => opt.MapFrom(_ => DateTime.Now))
            //    .ForMember(ja => ja.MatchScore,
            //    opt => opt.MapFrom(_ => (double?)null));

        }
    }
}
