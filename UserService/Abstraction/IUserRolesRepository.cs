using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IRolesRepository
    {
        dynamic GetRoles(string userRoleId, Pagination pageInfo);
        dynamic DeleteRoles(string id);
        dynamic InsertRoles(RolesModel model);
        dynamic UpdateRoles(RolesModel model);
    }
}