namespace Stock_Server.Service;
using  Stock_Server.Mapper;

public  interface IUserService
{
    Task<UserResponse> CreateUser(RegisterUserDto request);
}