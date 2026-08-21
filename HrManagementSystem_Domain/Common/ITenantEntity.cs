namespace HrMangmentSystem_Domain.Common
{
    public  interface ITenantEntity   //abstraction 
    {
       
        Guid TenantId { get; set; } 
    }
}
