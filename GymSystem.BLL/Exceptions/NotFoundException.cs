namespace GymSystem.BLL.Exceptions;

// بنرميها لما نطلب سجل مش موجود، والـ Middleware بيحولها لـ 404
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object? key)
        : base($"{entityName} with id '{key}' was not found.")
    {
    }
}