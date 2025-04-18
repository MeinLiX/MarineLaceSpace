namespace MarineLaceSpace.Exceptions.Repositories;

public class NotFoundEntityException : RepositoryBaseException
{
    public NotFoundEntityException() { }

    public NotFoundEntityException(string message) : base(message) { }

    public NotFoundEntityException(string message, Exception innerException) : base(message, innerException) { }
}
