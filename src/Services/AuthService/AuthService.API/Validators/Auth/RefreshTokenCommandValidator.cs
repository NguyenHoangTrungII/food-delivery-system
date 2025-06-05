//using AuthService.Application.Commands.Auth;
//using FluentValidation;

//namespace AuthService.Application.Validators.Auth;

//public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
//{
//    public RefreshTokenCommandValidator()
//    {
//        RuleFor(x => x.RefreshToken)
//            .NotEmpty().WithMessage("Refresh token is required.");
//    }
//}