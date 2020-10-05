using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public dynamic DeleteRoles(string id)
        {
            RolesResponse response = new RolesResponse();
            try
            {
                var roles = _context.Roles.Include(x => x.UsersRoles).Where(x => x.RoleId == Convert.ToInt32(id)).FirstOrDefault();
                if (roles == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleNotFound, StatusCodes.Status404NotFound);
            
                if (roles.UsersRoles != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleConflict, StatusCodes.Status409Conflict);
                
                {
                    _context.UsersRoles.RemoveRange(roles.UsersRoles);
                    _context.SaveChanges();
                }
                _context.Roles.Remove(roles);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.RoleDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetRoles(string userRoleId, Pagination pageInfo)
        {
            RolesGetResponse response = new RolesGetResponse();
            int totalCount = 0;
            try
            {
                List<RolesModel> userRolesModelList = new List<RolesModel>();
                if (userRoleId == "0")
                {
                    userRolesModelList = (from userRole in _context.Roles
                                          select new RolesModel()
                                          {
                                              RoleId = userRole.RoleId.ToString(),
                                              Application = userRole.Application,
                                              Privilege = userRole.Privilege
                                          }).OrderBy(a => a.RoleId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.ToList().Count();
                }
                else
                {
                    userRolesModelList = (from userRole in _context.Roles
                                          where userRole.RoleId == Convert.ToInt32(userRoleId)
                                          select new RolesModel()
                                          {
                                              RoleId = userRole.RoleId.ToString(),
                                              Application = userRole.Application,
                                              Privilege = userRole.Privilege
                                          }).OrderBy(a => a.RoleId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.Where(x => x.RoleId == Convert.ToInt32(userRoleId)).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.RoleRetrived;
                response.pagination = page;
                response.data = userRolesModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic InsertRoles(RolesModel model)
        {
            try
            {
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                Roles role = new Roles()
                {
                    Application = model.Application,
                    Privilege = model.Privilege
                };
                _context.Roles.Add(role);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.RoleInsert, true);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateRoles(RolesModel model)
        {
            try
            {
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var userRolesData = _context.Roles.Where(x => x.RoleId == Convert.ToInt32(model.RoleId)).FirstOrDefault();
                if (userRolesData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleNotFound, StatusCodes.Status404NotFound);

                userRolesData.Application = model.Application;
                userRolesData.Privilege = model.Privilege;
                _context.Roles.Update(userRolesData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.RoleUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}