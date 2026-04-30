using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Stock_Server.Mapper;
using Stock_Server.Models;
using Stock_Server.Repository;
using Stock_Server.Util.Exceptions;
using System.Threading.Tasks;

public class AuthService : IAuthService
{
    private readonly IUserRepo _repository;
    private readonly JSONUserMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IUserRepo repository,
        JSONUserMapper mapper,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    // ------------------------------------------------------------------
    // 1. Main authenticate method – uses PersistAuth
    // ------------------------------------------------------------------
    public async Task<LoginResponse> Authenticate(LoginRequest request)
    {   //step 1 - validate credentials
        var user = await _repository.GetByNameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Authentication failed: User {Username} not found", request.Username);
            throw new AuthException("Invalid username");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Authentication failed: Invalid password for {Username}", request.Username);
            throw new AuthException("Invalid password");
        }


         // Update user metadata
     await _repository.UpdateLoginMetadataAsync(user.Id, DateTime.UtcNow, true);
      

        // Generate token and set cookie
       
     var (accessToken, refreshToken, accessExpiry, refreshExpiry) = await PersistAuth(user);
        return new LoginResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role.ToString(),
            IsVerified = user.IsVerified,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = accessExpiry
        };
    }
    public async Task Logout(string Id)
    {
        await _repository.UpdateLogoutMetaDataAsync(Id,null,null) ;
           var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext != null)
    {
        httpContext.Response.Cookies.Delete("auth");
        httpContext.Response.Cookies.Delete("refreshToken");
    }

    }
    
public async Task<RefreshResponse> RefreshTokens(string refreshToken)
{
    // 1. Verify the refresh token
    var principal = VerifyRefreshToken(refreshToken);

    // 2. Extract user ID – use ClaimTypes.NameIdentifier (default mapping for "sub")
    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        _logger.LogWarning("Refresh token missing 'sub' claim");
        throw new InvalidTokenException("Invalid refresh token payload");
    }

    // 3. Get user from DB
    var user = await _repository.GetByIdAsync(userId);
    if (user == null)
    {
        _logger.LogWarning("User {UserId} not found for refresh token", userId);
        throw new ItemNotFoundException("User not found for the given refresh token");
    }

    // 4. Validate stored refresh token
    if (string.IsNullOrEmpty(user.RefreshToken) || user.RefreshTokenExpiryTime < DateTime.UtcNow)
    {
        _logger.LogWarning("Refresh token expired or missing for user {UserId}", userId);
        throw new AuthException("Refresh token expired or not found. Please log in again.");
    }

    // 5. Verify the token matches the stored hash
    if (!BCrypt.Net.BCrypt.Verify(refreshToken, user.RefreshToken))
    {
        _logger.LogWarning("Refresh token hash mismatch for user {UserId}", userId);
        throw new InvalidTokenException("Invalid refresh token. Please log in again.");
    }

    // 6. Rotate tokens
    var newAccessToken = GenerateAccessToken(user);
    var newRefreshToken = GenerateRefreshToken(user);
    var newHashedRefresh = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
    var newRefreshExpiry = DateTime.UtcNow.AddMinutes(
        double.Parse(GetJwtSetting("RefreshExpiryMinutes", "JWT_REFRESH_EXPIRY_MINUTES", "10080")));

    await _repository.UpdateRefreshTokenAsync(user.Id, newHashedRefresh, newRefreshExpiry);

    // 7. Set new cookies
    SetAccessCookie(newAccessToken);
    SetRefreshCookie(newRefreshToken);

    // 8. Return response
    var accessExpiryMinutes = double.Parse(GetJwtSetting("ExpiryMinutes", "JWT_EXPIRY_MINUTES", "60"));
    var accessExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutes);

    return new RefreshResponse
    {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken,
        ExpiresAt = accessExpiresAt
    };
}  
    // ------------------------------------------------------------------
    // 2. Generate JWT access token containing user ID + claims
    // ------------------------------------------------------------------
    private string GenerateAccessToken(User user)
    {
        // Get JWT settings from configuration (supports env vars)
        var secret = GetJwtSetting("SecretKey", "JWT_SECRET", "your-default-secret-key-at-least-32-chars");
        Console.WriteLine($"Access secret length: {secret.Length} characters"); // 
        var issuer = GetJwtSetting("Issuer", "JWT_ISSUER", "StockServer");
        var audience = GetJwtSetting("Audience", "JWT_AUDIENCE", "StockClient");
        var expiryMinutes = double.Parse(GetJwtSetting("ExpiryMinutes", "JWT_EXPIRY_MINUTES", "60"));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),          // unique user ID
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),           // user role (e.g., "Admin", "User") 
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


   private string GenerateRefreshToken(User user)
    {
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes( GetJwtSetting("RefreshSecretKey", "JWT_REFRESH_SECRET", "default-refresh-secret-min-32-chars")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: GetJwtSetting("Issuer", "JWT_ISSUER", "StockServer"),
            audience: GetJwtSetting("Audience", "JWT_AUDIENCE", "StockClient"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(GetJwtSetting("RefreshExpiryMinutes", "JWT_REFRESH_EXPIRY_MINUTES", "10080"))),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

// ---------- Token Verification (used later in middleware) ----------
    public ClaimsPrincipal VerifyAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes( GetJwtSetting("SecretKey", "JWT_SECRET", "default-access-secret-min-32-chars"));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        try
        {
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            throw new InvalidTokenException("Invalid access token");
        }
    }

     public ClaimsPrincipal VerifyRefreshToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes( GetJwtSetting("RefreshSecretKey", "JWT_REFRESH_SECRET", "default-refresh-secret-min-32-chars"));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        try
        {
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            throw new InvalidTokenException("Invalid refresh token");
        }
    }

    // ------------------------------------------------------------------
    // 3. Set HTTP‑only cookie named "auth" with the JWT token
    // ------------------------------------------------------------------
    private void SetAccessCookie(string token)
    {

        
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,        // prevents JavaScript access (XSS protection)
            Secure = false,          // send only over HTTPS (set to false only for local dev without HTTPS)
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(
                double.Parse(GetJwtSetting("ExpiryMinutes", "JWT_EXPIRY_MINUTES", "60"))
            )
        };
        // For local development with http (no certificate), you can temporarily set Secure = false
        // but never do this in production.

        httpContext.Response.Cookies.Append("auth", token, cookieOptions);
    }


    private void SetRefreshCookie(string token)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        httpContext.Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(
                double.Parse(GetJwtSetting("RefreshExpiryMinutes", "JWT_REFRESH_EXPIRY_MINUTES", "10080"))
            )
        });
    }
    // ------------------------------------------------------------------
    // 4. PersistAuth helper – calls both helpers and returns token + expiry
    // ------------------------------------------------------------------
    private async Task<(string AccessToken,string RefreshToken, DateTime AccessExpiresAt,DateTime RefExpiresAt)> PersistAuth(User user)
    {
        var access_token = GenerateAccessToken(user);
       

        var refresh_token = GenerateRefreshToken(user);
        var hashedRefresh=BCrypt.Net.BCrypt.HashPassword(refresh_token);
        var refresh_exp=DateTime.UtcNow.AddMinutes(double.Parse(GetJwtSetting("RefreshExpiryMinutes", "JWT_REFRESH_EXPIRY_MINUTES", "10080")));
        await _repository.UpdateRefreshTokenAsync(user.Id, hashedRefresh, refresh_exp);
         SetAccessCookie(access_token);
        SetRefreshCookie(refresh_token);

        var expiryMinutes = double.Parse(GetJwtSetting("ExpiryMinutes", "JWT_EXPIRY_MINUTES", "60"));
        var access_expat = DateTime.UtcNow.AddMinutes(expiryMinutes);
        return (access_token, refresh_token, access_expat, refresh_exp);
    
    }

    // ------------------------------------------------------------------
    // Helper to read JWT settings (first from environment, then from appsettings)
    // ------------------------------------------------------------------
    private string GetJwtSetting(string jsonKey, string envVarName, string defaultValue)
    {
        // Try environment variable first (for Docker / production)
        var envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            Console.WriteLine($"Using JWT setting from environment variable {envVarName}");
            return envValue;
        }

        // Fallback to appsettings.json
        var configValue = _configuration[$"JwtSettings:{jsonKey}"];
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            Console.WriteLine($"Using JWT setting from appsettings.json under {jsonKey}");
            return configValue;
        }


        return defaultValue;
    }

}