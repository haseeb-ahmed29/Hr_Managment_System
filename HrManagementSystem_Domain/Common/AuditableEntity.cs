namespace HrMangmentSystem_Domain.Common
{
    public interface IAuditableEntity  //abstraction 
    {
        DateTime CreatedAt { get; set; }
        Guid CreatedBy { get; set; }

        DateTime? UpdatedAt { get; set; }
        Guid? UpdatedBy { get; set; }

    }
}
