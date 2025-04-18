namespace MarineLaceSpace.Exceptions.Repositories;

public class UserManagerException : RepositoryExceptionBase
{
    public IEnumerable<string> Codes { get; private set; } = [];

    public UserManagerException(IEnumerable<string> codes) { Codes = codes; }

    public UserManagerException(string message) : base(message) { }

    public UserManagerException(string message, Exception innerException) : base(message, innerException) { }
}
