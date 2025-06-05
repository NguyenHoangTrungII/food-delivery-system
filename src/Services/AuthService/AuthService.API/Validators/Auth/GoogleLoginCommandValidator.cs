using AuthService.Application.Commands.Auth;
using FluentValidation;

namespace AuthService.Application.Validators.Auth;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID token is required.");
    }
}