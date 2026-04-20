using Stock_Server.Models;

namespace Stock_Server.Builder;

public sealed class UserBuilder
{
    private string? _id;
    private string? _username;
    private string? _passwordHash;
    private Roles? _role;
    private bool? _isVerified;
    private DateTime? _lastLogin;
    private string? _refreshToken;
    private DateTime? _refreshTokenExpiryTime;

    private UserBuilder() { }

    public static UserBuilder CreateBuilder() => new();

    public UserBuilder SetId(string id) { _id = id; return this; }
    public UserBuilder SetUsername(string username) { _username = username; return this; }
    public UserBuilder SetPasswordHash(string hash) { _passwordHash = hash; return this; }
    public UserBuilder SetRole(Roles role) { _role = role; return this; }
    public UserBuilder SetIsVerified(bool verified) { _isVerified = verified; return this; }
    public UserBuilder SetLastLogin(DateTime lastLogin) { _lastLogin = lastLogin; return this; }
    public UserBuilder SetRefreshToken(string token) { _refreshToken = token; return this; }
    public UserBuilder SetRefreshTokenExpiryTime(DateTime expiry) { _refreshTokenExpiryTime = expiry; return this; }

    public User Build()
    {
        var user = new User();   // defaults applied here

        if (_id != null) user.Id = _id;
        if (_username != null) user.Username = _username;
        if (_passwordHash != null) user.PasswordHash = _passwordHash;
        if (_role.HasValue) user.Role = _role.Value;
        if (_isVerified.HasValue) user.IsVerified = _isVerified.Value;
        if (_lastLogin.HasValue) user.LastLogin = _lastLogin.Value;
        if (_refreshToken != null) user.RefreshToken = _refreshToken;
        if (_refreshTokenExpiryTime.HasValue) user.RefreshTokenExpiryTime = _refreshTokenExpiryTime.Value;

        return user;
    }
}