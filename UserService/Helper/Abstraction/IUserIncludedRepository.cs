using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models.ResponseModel;

namespace UserService.Helper.Abstraction
{
    public interface IUserIncludedRepository
    {
        dynamic GetApplicationIncludedData(List<UsersModel> usersModelList);
        dynamic GetPrivilegeIncludedData(List<UsersModel> usersModelList);
    }
}
