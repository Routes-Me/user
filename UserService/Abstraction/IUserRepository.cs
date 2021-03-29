using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IUserRepository
    {
        dynamic DeleteUser(string userId);
        dynamic UpdateUser(UsersDto usersDto);
        dynamic GetUser(string userId, Pagination pageInfo, string includeType);
        dynamic PostUser(UsersDto usersDto);
    }
}
