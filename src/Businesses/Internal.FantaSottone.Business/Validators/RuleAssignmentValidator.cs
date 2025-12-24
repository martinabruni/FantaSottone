namespace Internal.FantaSottone.Business.Validators;

using FluentValidation;
using Internal.FantaSottone.Domain.Models;

public class RuleAssignmentValidator : AbstractValidator<RuleAssignment>
{
    public RuleAssignmentValidator()
    {
        RuleFor(x => x.RuleId).GreaterThan(0);
        RuleFor(x => x.GameId).GreaterThan(0);
        RuleFor(x => x.AssignedToPlayerId).GreaterThan(0);
        RuleFor(x => x.AssignedAt).NotEmpty();
        RuleFor(x => x.CreatedAt).NotEmpty();
        RuleFor(x => x.UpdatedAt).NotEmpty();
    }
}