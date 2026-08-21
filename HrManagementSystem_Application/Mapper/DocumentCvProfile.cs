using AutoMapper;
using HrMangmentSystem_Domain.Entities.Recruitment;
using HrMangmentSystem_Dto.DTOs.Job.Appilcation;

namespace HrMangmentSystem_Application.Mapper
{
    public class DocumentCvProfile : Profile
    {
        public DocumentCvProfile() 
        {
            CreateMap<DocumentCv , DocumentCvDto>().ReverseMap();

            CreateMap<CreateDocumentCvDto, DocumentCv>()
                .ForMember(d => d.UploadedAt,
                opt => opt.MapFrom(_ => DateTime.Now));
        }
    }
}
