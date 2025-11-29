namespace Common.Abstracts;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

