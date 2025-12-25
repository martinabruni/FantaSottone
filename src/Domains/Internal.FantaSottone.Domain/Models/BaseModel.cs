namespace Internal.FantaSottone.Domain.Models;

public interface IEntity
{
    public int Id { get; set; }
}

public abstract class BaseModel : IEntity
{
    public int Id { get; set; }
}
