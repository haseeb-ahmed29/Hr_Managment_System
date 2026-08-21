using AutoMapper;
using HrMangmentSystem_Domain.Tenants;
using HrMangmentSystem_Dto.DTOs.Tenant;

namespace HrMangmentSystem_Application.Mapper
{
    public class TenantProfile : Profile
    {
        public TenantProfile()
        {
            // DTO to Entity
            CreateMap<CreateTenantDto, TenantEntity>().ReverseMap();
              


            // DTO to Entity
            CreateMap<TenantEntity, TenantDto>().ReverseMap();


            // DTO to Entity for Update
            CreateMap<UpdateTenantDto, TenantEntity>();
             
        }
    }
}
