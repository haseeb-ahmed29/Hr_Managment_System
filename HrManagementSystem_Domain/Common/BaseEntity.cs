namespace HrMangmentSystem_Domain.Common
{
    public abstract  class BaseEntity<T> : IAuditableEntity 
    {

        public T Id { get; set; } = default!;
       
       public DateTime CreatedAt { get; set; } = DateTime.Now;
       

        public DateTime? UpdatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public Guid? UpdatedBy { get; set; }




    }
}
