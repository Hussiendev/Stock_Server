 using Stock_Server.Models;   
namespace Stock_Server.Mapper;
public record RegisterUserDto
{
  
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty; 
       public Roles? Role { get; set; }  
 
}
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class LoginResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Roles Role { get; set; }
    public bool IsVerified { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }   // access token expiry
}
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Roles Role { get; set; }
    public bool IsVerified { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    // No PasswordHash, No RefreshToken
}
public class UserSummaryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Roles Role { get; set; }
    public bool IsVerified { get; set; }
}