namespace MarineLaceSpace.Exceptions.Repositories;

public class ValidationEntityException : RepositoryExceptionBase
{
    public ValidationEntityException() { }

    public ValidationEntityException(string message) : base(message) { }

    public ValidationEntityException(string message, Exception innerException) : base(message, innerException) { }
}
