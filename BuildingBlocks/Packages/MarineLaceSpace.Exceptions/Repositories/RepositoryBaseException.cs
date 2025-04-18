namespace MarineLaceSpace.Exceptions.Repositories;

public abstract class RepositoryBaseException : ApplicationBaseException
{
    protected RepositoryBaseException() { }
    protected RepositoryBaseException(string message) : base(message) { }
    protected RepositoryBaseException(string message, Exception innerException) : base(message, innerException) { }
}
