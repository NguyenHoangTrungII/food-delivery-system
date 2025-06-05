using AuthService.Application.Queries.CheckUserPermission;
using FluentValidation;

namespace AuthService.Application.Validators;

public class CheckUserPermissionQueryValidator : AbstractValidator<CheckUserPermissionQuery>
{
    public CheckUserPermissionQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FunctionId)
            .NotEmpty().WithMessage("Function ID is required.");
    }
}