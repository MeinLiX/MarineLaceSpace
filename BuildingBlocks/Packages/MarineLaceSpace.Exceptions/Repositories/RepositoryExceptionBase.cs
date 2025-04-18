namespace MarineLaceSpace.Exceptions.Repositories;

public abstract class RepositoryExceptionBase : ApplicationBaseException
{
    protected RepositoryExceptionBase() { }
    protected RepositoryExceptionBase(string message) : base(message) { }
    protected RepositoryExceptionBase(string message, Exception innerException) : base(message, innerException) { }
}
