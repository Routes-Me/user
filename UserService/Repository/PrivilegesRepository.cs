using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Obfuscation;
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
    public class PrivilegesRepository : IPrivilegesRepository
    {
        private readonly UserServiceContext _context;
        private readonly AppSettings _appSettings;

        public PrivilegesRepository(IOptions<AppSettings> appSettings, UserServiceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }


        public dynamic DeletePrivilege(int id)
        {
            try
            {
                var PrivilegeIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(id), _appSettings.PrimeInverse);
                var privilegeData = _context.Privileges.Include(x => x.Roles).Where(x => x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (privilegeData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PrivilegeNotFound, StatusCodes.Status404NotFound);

                if (privilegeData.Roles.Count > 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.PrivilegeAssociatedWithRole, StatusCodes.Status409Conflict);

                var userRoleData= _context.UsersRoles.Where(x => x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (userRoleData != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PrivilegeAssociatedWithUserRole, StatusCodes.Status409Conflict);

                _context.Privileges.Remove(privilegeData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.PrivilegeDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetPrivilege(int id, Pagination pageInfo)
        {
            PrivilegesResponse response = new PrivilegesResponse();
            int totalCount = 0;
            try
            {
                var PrivilegeIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(id), _appSettings.PrimeInverse);
                List<PrivilegesModel> privilegesModelList = new List<PrivilegesModel>();
                if (PrivilegeIdDecrypted == 0)
                {
                    privilegesModelList = (from privilege in _context.Privileges
                                           select new PrivilegesModel()
                                           {
                                               PrivilegeId = ObfuscationClass.EncodeId(privilege.PrivilegeId, _appSettings.Prime).ToString(),
                                               Name = privilege.Name,
                                           }).AsEnumerable().OrderBy(a => a.PrivilegeId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.ToList().Count();
                }
                else
                {
                    privilegesModelList = (from privilege in _context.Privileges
                                           where privilege.PrivilegeId == PrivilegeIdDecrypted
                                           select new PrivilegesModel()
                                           {
                                               PrivilegeId = ObfuscationClass.EncodeId(privilege.PrivilegeId, _appSettings.Prime).ToString(),
                                               Name = privilege.Name,
                                           }).AsEnumerable().OrderBy(a => a.PrivilegeId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Roles.Where(x => x.PrivilegeId == PrivilegeIdDecrypted).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.PrivilegeRetrived;
                response.pagination = page;
                response.data = privilegesModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic PostPrivilege(PrivilegesModel model)
        {
            try
            {
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var privilegeData = _context.Privileges.Where(x => x.Name.ToLower() == model.Name.ToLower()).FirstOrDefault();
                if (privilegeData != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PrivilegeExists, StatusCodes.Status409Conflict);

                Privileges privileges = new Privileges()
                {
                    Name = model.Name
                };

                _context.Privileges.Add(privileges);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.PrivilegeInsert, true);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic PutPrivilege(PrivilegesModel model)
        {
            try
            {
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var PrivilegeIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.PrivilegeId), _appSettings.PrimeInverse);

                var privilegeData = _context.Privileges.Where(x => x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                if (privilegeData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PrivilegeNotFound, StatusCodes.Status404NotFound);

                var IsPrivilege = _context.Privileges.Where(x => x.Name.ToLower() == model.Name.ToLower() && x.PrivilegeId != PrivilegeIdDecrypted).FirstOrDefault();
                if (IsPrivilege != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PrivilegeExists, StatusCodes.Status409Conflict);

                privilegeData.Name = model.Name;
                _context.Privileges.Update(privilegeData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.PrivilegeUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}
