namespace Internal.FantaSottone.Business.Validators;

using FluentValidation;
using Internal.FantaSottone.Domain.Models;

public class RuleValidator : AbstractValidator<Rule>
{
    public RuleValidator()
    {
        RuleFor(x => x.GameId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RuleType).IsInEnum();

        // Fix #3: Validate ScoreDelta sign matches RuleType
        RuleFor(x => x.ScoreDelta)
            .GreaterThan(0)
            .When(x => x.RuleType == RuleType.Bonus)
            .WithMessage("Bonus rules must have a positive ScoreDelta");

        RuleFor(x => x.ScoreDelta)
            .LessThan(0)
            .When(x => x.RuleType == RuleType.Malus)
            .WithMessage("Malus rules must have a negative ScoreDelta");

        RuleFor(x => x.CreatedAt).NotEmpty();
        RuleFor(x => x.UpdatedAt).NotEmpty();
    }
}