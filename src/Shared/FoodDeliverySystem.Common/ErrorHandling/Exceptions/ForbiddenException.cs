namespace FoodDeliverySystem.Common.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message)
        : base(message, "FORBIDDEN", 403)
    {
    }
}