namespace Stock_Server.Util.Exceptions;

public class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message) : base(message)
    {
    }
}

public class InvalidItemException : Exception
{
    public InvalidItemException(string message) : base(message)
    {
    }
}

public class RepositoryInitializationException : Exception
{
    public RepositoryInitializationException(string message, Exception innerException)
        : base($"{message}: {innerException.Message}", innerException)
    {
    }
}

public class DBException : Exception
{
    public DBException(string message, Exception innerException)
        : base($"{message}: {innerException.Message}", innerException)
    {
    }
}