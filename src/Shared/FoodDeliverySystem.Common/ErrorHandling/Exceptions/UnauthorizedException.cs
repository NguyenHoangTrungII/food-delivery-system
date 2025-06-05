namespace FoodDeliverySystem.Common.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message)
        : base(message, "UNAUTHORIZED", 401)
    {
    }
}