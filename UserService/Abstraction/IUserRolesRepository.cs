using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IRolesRepository
    {
        dynamic GetRoles(string ApplicationId, string PrivilegeId, Pagination pageInfo);
        dynamic DeleteRoles(string ApplicationId, string PrivilegeId);
        dynamic InsertRoles(RolesModel model);
        dynamic UpdateRoles(RolesModel model);
    }
}