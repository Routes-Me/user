using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IUserRepository
    {
        dynamic DeleteUser(int id);
        dynamic UpdateUser(RegistrationModel model);
        dynamic GetUser(int userId, Pagination pageInfo);
    }
}
