namespace Internal.FantaSottone.Business.Validators;

using FluentValidation;
using Internal.FantaSottone.Domain.Models;

public class PlayerValidator : AbstractValidator<Player>
{
    public PlayerValidator()
    {
        //RuleFor(x => x.GameId).GreaterThan(0);
        RuleFor(x => x.CreatedAt).NotEmpty();
        RuleFor(x => x.UpdatedAt).NotEmpty();
    }
}