using System.Security.Claims;
using Stock_Server.Mapper;
public interface IAuthService

{
    Task<LoginResponse> Authenticate(LoginRequest request);
    ClaimsPrincipal VerifyAccessToken(string token);
    Task Logout(string ID);
    Task<RefreshResponse> RefreshTokens(string refreshToken);
    
}