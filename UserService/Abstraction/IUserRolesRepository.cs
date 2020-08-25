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
        RolesGetResponse GetRoles(int userRoleId, Pagination pageInfo);
        RolesResponse DeleteRoles(int id);
        RolesResponse InsertRoles(RolesModel model);
        RolesResponse UpdateRoles(RolesModel model);
    }
}