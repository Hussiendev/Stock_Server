using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stock_Server.Mapper;
using Stock_Server.Util;
using Stock_Server.Util.Exceptions;
namespace Stock_Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController:ControllerBase{
    private readonly IAuthService _authService;


    public AuthController(IAuthService authService)
    {
        _authService = authService;
     
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var Loginresp = await _authService.Authenticate(request);
            return Ok(Loginresp);
        }
        catch (AuthException ex)
        {
            return StatusCode(401,ex.Message);
        }
    }

[HttpPost("logout")]
[Authorize]   // Only authenticated users can logout
public async Task<IActionResult> Logout()
{
      var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
              ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirstValue("sub");
    if (string.IsNullOrEmpty(userId))
        return Unauthorized(new { message = "User not identified" });

    await _authService.Logout(userId);
    return Ok(new { message = "Logged out successfully" });
}
[HttpPost("refresh")]
[AllowAnonymous]   // because access token may be expired
public async Task<IActionResult> Refresh()
{
    var refreshToken = Request.Cookies["refreshToken"];
    if (string.IsNullOrEmpty(refreshToken))
        return BadRequest(new { message = "Refresh token missing" });

    try
    {
        var result = await _authService.RefreshTokens(refreshToken);
        return Ok(result);
    }
    catch (InvalidTokenException ex)
    {
        return Unauthorized(new { message = ex.Message });
    }
}
  
}

