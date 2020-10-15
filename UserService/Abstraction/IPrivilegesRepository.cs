using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IPrivilegesRepository
    {
        dynamic PostPrivilege(PrivilegesModel model);
        dynamic PutPrivilege(PrivilegesModel model);
        dynamic GetPrivilege(int id, Pagination pageInfo);
        dynamic DeletePrivilege(int id);
    }
}
