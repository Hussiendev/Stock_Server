using Stock_Server.Builder;
using Stock_Server.Models;
using Stock_Server.Util;
using BCrypt.Net;  

namespace Stock_Server.Mapper;

public class JSONUserMapper : IMapper<RegisterUserDto, User>
{
    public User Map(RegisterUserDto input)
    {
        var builder = UserBuilder.CreateBuilder()
            .SetId(IdGenerator.Generate("user"))
            .SetUsername(input.Username)
            .SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(input.Password));

        // Only set role if client provided it, otherwise default (Employee) will be used
        if (input.Role.HasValue)
            builder.SetRole(input.Role.Value);

        // Do NOT set IsVerified, LastLogin, RefreshToken, etc. – they will keep defaults
        return builder.Build();
    }

    public RegisterUserDto ReverseMap(User input)
    {
        return new RegisterUserDto
        {
            Username = input.Username
            
            // Password is never returned
            // Role could be returned if needed
        };
    }
}