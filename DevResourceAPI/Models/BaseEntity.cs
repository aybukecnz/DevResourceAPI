public class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SpecialBaseEntity : BaseEntity
{
    public string SpecialProperty { get; set; } = string.Empty;
}

public class AnotherEntity : SpecialBaseEntity
{
    public int AnotherProperty { get; set; }
}