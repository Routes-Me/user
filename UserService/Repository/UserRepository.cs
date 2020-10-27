using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Obfuscation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UserService.Abstraction;
using UserService.Helper.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly userserviceContext _context;
        private readonly AppSettings _appSettings;
        private readonly IUserIncludedRepository _userIncludedRepository;
        private readonly Dependencies _dependencies;
        public UserRepository(IOptions<AppSettings> appSettings, userserviceContext context, IUserIncludedRepository userIncludedRepository, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _userIncludedRepository = userIncludedRepository;
            _dependencies = dependencies.Value;
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

        public dynamic GetUser(string userId, Pagination pageInfo, string includeType)
        {
            try
            {
                var userIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(userId), _appSettings.PrimeInverse);
                int totalCount = 0;
                UsersGetResponse response = new UsersGetResponse();
                List<UsersModel> usersModelList = new List<UsersModel>();
                var driverData = GetInstitutionIdsFromDrivers();
                if (userIdDecrypted == 0)
                {
                    usersModelList = (from user in _context.Users
                                      join phone in _context.Phones on user.UserId equals phone.UserId
                                      select new UsersModel
                                      {
                                          UserId = ObfuscationClass.EncodeId(user.UserId, _appSettings.Prime).ToString(),
                                          Phone = phone.Number,
                                          Email = user.Email,
                                          CreatedAt = user.CreatedAt,
                                          Name = user.Name,
                                          Roles = null
                                      }).AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    List<UsersModel> users = new List<UsersModel>();
                    for (int i = 0; i < usersModelList.Count(); i++)
                    {
                        UsersModel user = new UsersModel();
                        user.UserId = usersModelList[i].UserId;
                        user.Phone = usersModelList[i].Phone;
                        user.Email = usersModelList[i].Email;
                        user.CreatedAt = usersModelList[i].CreatedAt;
                        user.Name = usersModelList[i].Name;
                        var usersRoles = (from userroles in _context.UsersRoles
                                          where userroles.UserId == ObfuscationClass.DecodeId(Convert.ToInt32(usersModelList[i].UserId), _appSettings.PrimeInverse)
                                          select new RolesModel
                                          {
                                              ApplicationId = ObfuscationClass.EncodeId(userroles.ApplicationId, _appSettings.Prime).ToString(),
                                              PrivilegeId = ObfuscationClass.EncodeId(userroles.PrivilegeId, _appSettings.Prime).ToString()
                                          }).ToList();
                        user.Roles = usersRoles;
                        user.InstitutionId = driverData.Where(x => x.UserId == usersModelList[i].UserId).Select(x => x.InstitutionId).FirstOrDefault();
                        users.Add(user);
                    }
                    usersModelList = new List<UsersModel>();
                    usersModelList = users;

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
                                 where user.UserId == userIdDecrypted
                                 select new RolesModel
                                 {
                                     ApplicationId = ObfuscationClass.EncodeId(usersRole.ApplicationId, _appSettings.Prime).ToString(),
                                     PrivilegeId = ObfuscationClass.EncodeId(usersRole.PrivilegeId, _appSettings.Prime).ToString()
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
                                          InstitutionId = driverData.Where(x => x.UserId == ObfuscationClass.EncodeId(user.UserId, _appSettings.Prime).ToString()).Select(x => x.InstitutionId).FirstOrDefault(),
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

                dynamic includeData = new JObject();
                if (!string.IsNullOrEmpty(includeType))
                {
                    string[] includeArr = includeType.Split(',');
                    if (includeArr.Length > 0)
                    {
                        foreach (var item in includeArr)
                        {
                            if (item.ToLower() == "application" || item.ToLower() == "applications")
                                includeData.applications = _userIncludedRepository.GetApplicationIncludedData(usersModelList);

                            else if (item.ToLower() == "privilege" || item.ToLower() == "privileges")
                                includeData.privileges = _userIncludedRepository.GetPrivilegeIncludedData(usersModelList);
                        }
                    }
                }

                if (((JContainer)includeData).Count == 0)
                    includeData = null;

                response.message = CommonMessage.UserRetrived;
                response.statusCode = StatusCodes.Status200OK;
                response.status = true;
                response.pagination = page;
                response.data = usersModelList;
                response.included = includeData;

                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public List<DriversGetModel> GetInstitutionIdsFromDrivers()
        {
            List<DriversGetModel> lstDrivers = new List<DriversGetModel>();
            try
            {
                var client = new RestClient(_appSettings.Host + _dependencies.VehicleUrl);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content;
                    var driverData = JsonConvert.DeserializeObject<DriverGetResponse>(result);
                    lstDrivers.AddRange(driverData.data);
                }
                return lstDrivers;
            }
            catch (Exception ex)
            {
                return lstDrivers;
            }
        }
    }
}