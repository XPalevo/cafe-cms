namespace Common.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    public void SetUpdatedAt()
    {
        this.UpdatedAt = DateTime.UtcNow;
    }
}