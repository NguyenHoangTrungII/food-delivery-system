using FluentValidation;
using AuthService.Application.Commands.Auth;

namespace AuthService.Application.Validators;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .Length(3, 50).WithMessage("Role name must be between 3 and 50 characters.")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Role name can only contain letters, numbers, underscores, or hyphens.");
    }
}