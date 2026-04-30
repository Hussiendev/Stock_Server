namespace Stock_Server.Util.Exceptions;
public class AuthException : Exception
{
    public AuthException(string message) : base(message)
    {
    }
   
}
public class InvalidTokenException : Exception
{
    public InvalidTokenException(string message) : base(message)
    {
    }
}