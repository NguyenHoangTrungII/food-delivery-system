namespace FoodDeliverySystem.Common.Exceptions;

public class AuthException : BaseException
{
    public AuthException(string message, Exception? innerException = null)
        : base(message, "AUTH_ERROR", 400, null, innerException)
    {
    }
}