namespace FoodDeliverySystem.Common.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string resourceType, int resourceId)
        : base($"{resourceType} with ID {resourceId} not found.", "NOT_FOUND", 404)
    {
    }
}