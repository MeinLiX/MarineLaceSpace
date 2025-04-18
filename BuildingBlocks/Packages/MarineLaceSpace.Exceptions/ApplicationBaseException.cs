namespace MarineLaceSpace.Exceptions;

public abstract class ApplicationBaseException : Exception
{
    protected ApplicationBaseException() { }
    protected ApplicationBaseException(string message) : base(message) { }
    protected ApplicationBaseException(string message, Exception innerException) : base(message, innerException) { }
}
