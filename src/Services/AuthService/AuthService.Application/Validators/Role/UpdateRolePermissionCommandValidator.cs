using FluentValidation;
using AuthService.Application.Commands.Auth;

namespace AuthService.Application.Validators;

public class UpdateRolePermissionCommandValidator : AbstractValidator<UpdateRolePermissionCommand>
{
    public UpdateRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Role ID cannot be empty.");

        RuleFor(x => x.FunctionId)
            .NotEmpty().WithMessage("Function ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Function ID cannot be empty.");
    }
}