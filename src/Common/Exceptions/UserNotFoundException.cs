namespace Common.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException() : base("Пользователь не найден")
    {
    }

    public UserNotFoundException(string message) : base(message)
    {
    }

    public UserNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
