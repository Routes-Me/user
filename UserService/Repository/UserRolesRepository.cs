﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class UserRolesRepository : IRolesRepository
    {
        private readonly userserviceContext _context;
        private readonly AppSettings _appSettings;

        public UserRolesRepository(IOptions<AppSettings> appSettings, userserviceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public dynamic DeleteRoles(string ApplicationId, string PrivilegeId)
        {
            RolesResponse response = new RolesResponse();
            try
            {
                var ApplicationIdDecrypted = Obfuscation.Decode(ApplicationId);
                var PrivilegeIdDecrypted = Obfuscation.Decode(PrivilegeId);
                var roles = _context.Roles.Where(x => x.ApplicationId == ApplicationIdDecrypted && x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (roles == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleNotFound, StatusCodes.Status404NotFound);

                var usersRole = _context.UsersRoles.Where(x => x.ApplicationId == ApplicationIdDecrypted && x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (usersRole != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleConflict, StatusCodes.Status409Conflict);

                _context.UsersRoles.RemoveRange(usersRole);
                _context.SaveChanges();
                _context.Roles.Remove(roles);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.RoleDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetRoles(string ApplicationId, string PrivilegeId, Pagination pageInfo)
        {
            RolesGetResponse response = new RolesGetResponse();
            int totalCount = 0;
            try
            {
                var ApplicationIdDecrypted = Obfuscation.Decode(ApplicationId);
                var PrivilegeIdDecrypted = Obfuscation.Decode(PrivilegeId);
                List<RolesModel> userRolesModelList = new List<RolesModel>();
                if (ApplicationIdDecrypted == 0 && PrivilegeIdDecrypted == 0)
                {
                    userRolesModelList = (from userRole in _context.Roles
                                          select new RolesModel()
                                          {
                                              ApplicationId = Obfuscation.Encode(userRole.ApplicationId).ToString(),
                                              PrivilegeId = Obfuscation.Encode(userRole.PrivilegeId).ToString(),
                                          }).AsEnumerable().OrderBy(a => a.ApplicationId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.ToList().Count();
                }
                else
                {
                    userRolesModelList = (from userRole in _context.Roles
                                          where userRole.PrivilegeId == PrivilegeIdDecrypted && userRole.ApplicationId == ApplicationIdDecrypted
                                          select new RolesModel()
                                          {
                                              ApplicationId = Obfuscation.Encode(userRole.ApplicationId).ToString(),
                                              PrivilegeId = Obfuscation.Encode(userRole.PrivilegeId).ToString(),
                                          }).AsEnumerable().OrderBy(a => a.ApplicationId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.Where(x => x.PrivilegeId == PrivilegeIdDecrypted && x.ApplicationId == ApplicationIdDecrypted).ToList().Count();
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

                var ApplicationIdDecrypted = Obfuscation.Decode(model.ApplicationId);
                var PrivilegeIdDecrypted = Obfuscation.Decode(model.PrivilegeId);

                Roles role = new Roles()
                {
                    ApplicationId = ApplicationIdDecrypted,
                    PrivilegeId = PrivilegeIdDecrypted
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

                var ApplicationIdDecrypted = Obfuscation.Decode(model.ApplicationId);
                var PrivilegeIdDecrypted = Obfuscation.Decode(model.PrivilegeId);

                var roles = _context.Roles.Where(x => x.ApplicationId == ApplicationIdDecrypted && x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (roles == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.RoleNotFound, StatusCodes.Status404NotFound);

                roles.ApplicationId = ApplicationIdDecrypted;
                roles.PrivilegeId = PrivilegeIdDecrypted;
                _context.Roles.Update(roles);
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