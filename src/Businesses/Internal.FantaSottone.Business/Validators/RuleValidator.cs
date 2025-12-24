namespace Internal.FantaSottone.Business.Validators;

using FluentValidation;
using Internal.FantaSottone.Domain.Models;

public class RuleValidator : AbstractValidator<Rule>
{
    public RuleValidator()
    {
        RuleFor(x => x.GameId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CreatedAt).NotEmpty();
        RuleFor(x => x.UpdatedAt).NotEmpty();
    }
}