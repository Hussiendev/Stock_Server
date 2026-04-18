namespace Stock_Server.Util;

public static class IdGenerator
{
    public static string Generate(string? prefix = null)
    {
        var guid = Guid.NewGuid().ToString();
        return prefix != null ? $"{prefix}-{guid}" : guid;
    }
}