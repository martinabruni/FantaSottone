namespace Internal.FantaSottone.Business.Validators;

using FluentValidation;
using Internal.FantaSottone.Domain.Models;

public class GameValidator : AbstractValidator<Game>
{
    public GameValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InitialScore).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CreatedAt).NotEmpty();
        RuleFor(x => x.UpdatedAt).NotEmpty();
    }
}