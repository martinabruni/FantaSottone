namespace Internal.FantaSottone.Domain.Validators;

public interface IValidator<T> where T : class
{
    void Validate(T model);
}