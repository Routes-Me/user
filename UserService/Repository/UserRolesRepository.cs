using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class UserRolesRepository : IRolesRepository
    {
        private readonly userserviceContext _context;
        public UserRolesRepository(userserviceContext context)
        {
            _context = context;
        }

        public RolesResponse DeleteRoles(int id)
        {
            RolesResponse response = new RolesResponse();
            try
            {
                var roles = _context.Roles.Where(x => x.RoleId == id).FirstOrDefault();
                if (roles == null)
                {
                    response.status = false;
                    response.message = "Role not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var userRoleData = _context.UsersRoles.Where(x => x.RoleId == id).FirstOrDefault();
                if (userRoleData != null)
                {
                    response.status = false;
                    response.message = "Role is associated with other user.";
                    response.responseCode = ResponseCode.Conflict;
                    return response;
                }

                _context.Roles.Remove(roles);
                _context.SaveChanges();
                response.status = true;
                response.message = "Role deleted successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting role. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public RolesGetResponse GetRoles(int userRoleId, Pagination pageInfo)
        {
            RolesGetResponse response = new RolesGetResponse();
            int totalCount = 0;
            try
            {
                List<RolesModel> userRolesModelList = new List<RolesModel>();

                if (userRoleId == 0)
                {
                    userRolesModelList = (from userRole in _context.Roles
                                          select new RolesModel()
                                          {
                                              RoleId = userRole.RoleId,
                                              Application = userRole.Application,
                                              Description = userRole.Description,
                                              Name = userRole.Name
                                          }).OrderBy(a => a.RoleId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.ToList().Count();
                }
                else
                {
                    userRolesModelList = (from userRole in _context.Roles
                                          where userRole.RoleId == userRoleId
                                          select new RolesModel()
                                          {
                                              RoleId = userRole.RoleId,
                                              Application = userRole.Application,
                                              Description = userRole.Description,
                                              Name = userRole.Name
                                          }).OrderBy(a => a.RoleId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.Where(x => x.RoleId == userRoleId).ToList().Count();
                }

                if (userRolesModelList == null || userRolesModelList.Count == 0)
                {
                    response.status = false;
                    response.message = "Role not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };


                response.status = true;
                response.message = "Roles data retrived successfully.";
                response.pagination = page;
                response.data = userRolesModelList;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while fetching roles. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public RolesResponse InsertRoles(RolesModel model)
        {
            RolesResponse response = new RolesResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                Roles role = new Roles()
                {
                    Application = model.Application,
                    Description = model.Description,
                    Name = model.Name
                };

                _context.Roles.Add(role);
                _context.SaveChanges();
                response.status = true;
                response.message = "User role inserted successfully.";
                response.responseCode = ResponseCode.Created;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting user role. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public RolesResponse UpdateRoles(RolesModel model)
        {
            RolesResponse response = new RolesResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var userRolesData = _context.Roles.Where(x => x.RoleId == model.RoleId).FirstOrDefault();
                if (userRolesData == null)
                {
                    response.status = false;
                    response.message = "User role not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                userRolesData.Application = model.Application;
                userRolesData.Description = model.Description;
                userRolesData.Name = model.Name;
                _context.Roles.Update(userRolesData);
                _context.SaveChanges();
                response.status = true;
                response.message = "Role updated successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating role. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
