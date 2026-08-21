namespace HrMangmentSystem_Domain.Common
{
    public  class  SoftDeletable<T>  : TenantEntity<T> 
    {
        public bool IsDeleted { get; set; } 

        public DateTime DeletedAt  { get; set; } 

        public Guid? DeletedByEmployeeId { get; set; }
    }
}
