namespace Common.Exceptions;

public class InvalidPasswordException : Exception
{
    public InvalidPasswordException() : base("Неверный пароль")
    {
    }

    public InvalidPasswordException(string message) : base(message)
    {
    }

    public InvalidPasswordException(string message, Exception innerException) : base(message, innerException)
    {
    }
}