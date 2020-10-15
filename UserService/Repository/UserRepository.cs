using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Obfuscation;
using System;
using System.Collections.Generic;
using System.Linq;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserServiceContext _context;
        private readonly AppSettings _appSettings;

        public UserRepository(IOptions<AppSettings> appSettings, UserServiceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public dynamic DeleteUser(string id)
        {
            try
            {
                var userIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(id), _appSettings.PrimeInverse);
                var users = _context.Users.Include(x => x.Phones).Include(x => x.UsersRoles).Where(x => x.UserId == userIdDecrypted).FirstOrDefault();
                if (users == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status404NotFound);

                if (users.UsersRoles != null)
                {
                    _context.UsersRoles.RemoveRange(users.UsersRoles);
                    _context.SaveChanges();
                }
                if (users.Phones != null)
                {
                    _context.Phones.RemoveRange(users.Phones);
                    _context.SaveChanges();
                }
                _context.Users.Remove(users);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.UserDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateUser(RegistrationModel model)
        {
            try
            {
                var userIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.UserId), _appSettings.PrimeInverse);
                List<RolesModel> roles = new List<RolesModel>();
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var user = _context.Users.Include(x => x.UsersRoles).Include(x => x.Phones).Where(x => x.UserId == userIdDecrypted).FirstOrDefault();
                if (user == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status404NotFound);

                foreach (var role in model.Roles)
                {
                    var userRole = _context.Roles.Where(x => x.ApplicationId == ObfuscationClass.DecodeId(Convert.ToInt32(role.ApplicationId), _appSettings.PrimeInverse)
                    && x.PrivilegeId == ObfuscationClass.DecodeId(Convert.ToInt32(role.PrivilegeId), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (userRole == null)
                    {
                        return ReturnResponse.ErrorResponse(CommonMessage.UserRoleNotFound, StatusCodes.Status404NotFound);
                    }
                    else
                    {
                        RolesModel rolesModel = new RolesModel();
                        rolesModel.ApplicationId = ObfuscationClass.EncodeId(userRole.ApplicationId, _appSettings.Prime).ToString();
                        rolesModel.PrivilegeId = ObfuscationClass.EncodeId(userRole.PrivilegeId, _appSettings.Prime).ToString();
                        roles.Add(rolesModel);
                    }
                }

                if (user.UsersRoles != null)
                {
                    _context.UsersRoles.RemoveRange(user.UsersRoles);
                    _context.SaveChanges();
                }

                foreach (var role in roles)
                {
                    UsersRoles usersroles = new UsersRoles()
                    {
                        UserId = userIdDecrypted,
                        ApplicationId = ObfuscationClass.DecodeId(Convert.ToInt32(role.ApplicationId), _appSettings.PrimeInverse),
                        PrivilegeId = ObfuscationClass.DecodeId(Convert.ToInt32(role.PrivilegeId), _appSettings.PrimeInverse)
                    };
                    _context.UsersRoles.Add(usersroles);
                }
                _context.SaveChanges();

                var userPhone = user.Phones.Where(x => x.UserId == userIdDecrypted).FirstOrDefault();
                if (userPhone == null)
                {
                    Phones newPhone = new Phones()
                    {
                        IsVerified = false,
                        Number = model.PhoneNumber,
                        UserId = userIdDecrypted
                    };
                    _context.Phones.Add(newPhone);
                }
                else if (userPhone.Number != model.PhoneNumber)
                {
                    userPhone.Number = model.PhoneNumber;
                    userPhone.IsVerified = false;
                    _context.Phones.Update(userPhone);
                    _context.SaveChanges();
                }

                if (user.Email != model.Email)
                    user.IsEmailVerified = false;

                user.Name = model.Name;
                user.Email = model.Email;
                _context.Users.Update(user);
                _context.SaveChanges();

                return ReturnResponse.SuccessResponse(CommonMessage.UserUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetUser(string userId, Pagination pageInfo)
        {
            try
            {
                var userIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(userId), _appSettings.PrimeInverse);
                int totalCount = 0;
                UsersGetResponse response = new UsersGetResponse();
                List<UsersModel> usersModelList = new List<UsersModel>();
                if (userIdDecrypted == 0)
                {
                    var roles = (from user in _context.Users
                                 join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                 join role in _context.Roles on new { ApplicationId = usersRole.ApplicationId, PrivilegeId = usersRole.PrivilegeId } equals new { ApplicationId = role.ApplicationId, PrivilegeId = role.PrivilegeId }
                                 select new RolesModel
                                 {
                                     ApplicationId = ObfuscationClass.EncodeId(role.ApplicationId, _appSettings.Prime).ToString(),
                                     PrivilegeId = ObfuscationClass.EncodeId(role.PrivilegeId, _appSettings.Prime).ToString()
                                 }).ToList();

                    usersModelList = (from user in _context.Users
                                      join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                      join role in _context.Roles on new { ApplicationId = usersRole.ApplicationId, PrivilegeId = usersRole.PrivilegeId } equals new { ApplicationId = role.ApplicationId, PrivilegeId = role.PrivilegeId }
                                      join phone in _context.Phones on user.UserId equals phone.UserId
                                      select new UsersModel
                                      {
                                          UserId = ObfuscationClass.EncodeId(user.UserId, _appSettings.Prime).ToString(),
                                          Phone = phone.Number,
                                          Email = user.Email,
                                          CreatedAt = user.CreatedAt,
                                          Name = user.Name,
                                          Roles = roles,
                                      }).AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from user in _context.Users
                                  join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                  join role in _context.Roles on new { ApplicationId = usersRole.ApplicationId, PrivilegeId = usersRole.PrivilegeId } equals new { ApplicationId = role.ApplicationId, PrivilegeId = role.PrivilegeId }
                                  join phone in _context.Phones on user.UserId equals phone.UserId
                                  select new UsersModel { }).AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().Count();
                }
                else
                {
                    var roles = (from user in _context.Users
                                 join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                 join role in _context.Roles on new { ApplicationId = usersRole.ApplicationId, PrivilegeId = usersRole.PrivilegeId } equals new { ApplicationId = role.ApplicationId, PrivilegeId = role.PrivilegeId }
                                 where user.UserId == userIdDecrypted
                                 select new RolesModel
                                 {
                                     ApplicationId = ObfuscationClass.EncodeId(role.ApplicationId, _appSettings.Prime).ToString(),
                                     PrivilegeId = ObfuscationClass.EncodeId(role.PrivilegeId, _appSettings.Prime).ToString()
                                 }).ToList();

                    usersModelList = (from user in _context.Users
                                      join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                      join role in _context.Roles on new { ApplicationId = usersRole.ApplicationId, PrivilegeId = usersRole.PrivilegeId } equals new { ApplicationId = role.ApplicationId, PrivilegeId = role.PrivilegeId }
                                      join phone in _context.Phones on user.UserId equals phone.UserId
                                      where user.UserId == userIdDecrypted
                                      select new UsersModel
                                      {
                                          UserId = ObfuscationClass.EncodeId(user.UserId, _appSettings.Prime).ToString(),
                                          Phone = phone.Number,
                                          Email = user.Email,
                                          CreatedAt = user.CreatedAt,
                                          Name = user.Name,
                                          Roles = roles,
                                      }).AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from user in _context.Users
                                  join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                  join role in _context.Roles on new { ApplicationId = usersRole.ApplicationId, PrivilegeId = usersRole.PrivilegeId } equals new { ApplicationId = role.ApplicationId, PrivilegeId = role.PrivilegeId }
                                  join phone in _context.Phones on user.UserId equals phone.UserId
                                  where user.UserId == userIdDecrypted
                                  select new UsersModel { }).AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.message = CommonMessage.UserRetrived;
                response.status = true;
                response.pagination = page;
                response.data = usersModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}