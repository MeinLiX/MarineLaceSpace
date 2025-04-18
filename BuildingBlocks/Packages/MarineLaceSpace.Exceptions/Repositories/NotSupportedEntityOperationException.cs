namespace MarineLaceSpace.Exceptions.Repositories;

public class NotSupportedEntityOperationException : RepositoryExceptionBase
{
    public NotSupportedEntityOperationException() { }

    public NotSupportedEntityOperationException(string message) : base(message) { }

    public NotSupportedEntityOperationException(string message, Exception innerException) : base(message, innerException) { }
}
