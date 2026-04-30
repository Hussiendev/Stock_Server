namespace Stock_Server.Service;
using  Stock_Server.Mapper;

public  interface IUserService
{
    Task<UserResponse> CreateUser(RegisterUserDto request);
Task <UserSummaryResponse> GetUserBYName(string name);
 Task DeleteUser(string id);
Task<IEnumerable<UserSummaryResponse>> GetAll();
  
Task Update_User(string name,UserUpdateRequest req);
    
}