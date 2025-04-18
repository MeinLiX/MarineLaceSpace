namespace MarineLaceSpace.Exceptions.Repositories;

public class DuplicateEntityException : RepositoryBaseException
{
    public DuplicateEntityException() { }

    public DuplicateEntityException(string message) : base(message) { }

    public DuplicateEntityException(string message, Exception innerException) : base(message, innerException) { }
}
