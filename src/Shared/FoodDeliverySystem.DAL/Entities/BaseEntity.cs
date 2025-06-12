namespace FoodDeliverySystem.DataAccess.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    //public DateTime? UpdatedAt { get; set; }
    //public string? UpdatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}


//public abstract class BaseEntity
//{
//    public Guid Id { get; set; } = Guid.NewGuid();
//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? UpdatedAt { get; set; }
//    public bool IsDeleted { get; set; }
//}