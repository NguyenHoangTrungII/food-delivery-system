//using AuthService.Application.Commands.Auth;
//using FluentValidation;

//namespace AuthService.Application.Validators.Auth;

//public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
//{
//    public ForgotPasswordCommandValidator()
//    {
//        RuleFor(x => x.Email)
//            .NotEmpty().WithMessage("Email is required.")
//            .EmailAddress().WithMessage("Invalid email format.");
//    }
//}