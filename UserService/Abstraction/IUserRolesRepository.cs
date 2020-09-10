using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IRolesRepository
    {
        dynamic GetRoles(int userRoleId, Pagination pageInfo);
        dynamic DeleteRoles(int id);
        dynamic InsertRoles(RolesModel model);
        dynamic UpdateRoles(RolesModel model);
    }
}