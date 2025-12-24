namespace Internal.FantaSottone.Business.Validators;

using Internal.FantaSottone.Domain.Validators;

public abstract class BaseValidator<T> : IValidator<T> where T : class
{
    protected BaseValidator()
    {
    }

    public abstract void Validate(T model);
}