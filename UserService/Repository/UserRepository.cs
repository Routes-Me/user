using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly userserviceContext _context;
        public UserRepository(userserviceContext context)
        {
            _context = context;
        }

        public dynamic DeleteUser(string id)
        {
            try
            {
                var users = _context.Users.Include(x => x.Phones).Include(x => x.UsersRoles).Where(x => x.UserId == Convert.ToInt32(id)).FirstOrDefault();
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
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var user = _context.Users.Include(x => x.UsersRoles).Include(x => x.Phones).Where(x => x.UserId == Convert.ToInt32(model.UserId)).FirstOrDefault();
                if (user == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status404NotFound);

                foreach (var role in model.Roles)
                {
                    var userRoles = _context.Roles.Where(x => x.RoleId == role).FirstOrDefault();
                    if (userRoles == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.UserRoleNotFound, StatusCodes.Status404NotFound);
                }

                if (user.UsersRoles != null)
                {
                    _context.UsersRoles.RemoveRange(user.UsersRoles);
                    _context.SaveChanges();
                }

                foreach (var role in model.Roles)
                {
                    UsersRoles usersroles = new UsersRoles()
                    {
                        UserId = Convert.ToInt32(model.UserId),
                        RoleId = role
                    };
                    _context.UsersRoles.Add(usersroles);
                }
                _context.SaveChanges();

                var userPhone = user.Phones.Where(x => x.UserId == Convert.ToInt32(model.UserId)).FirstOrDefault();
                if (userPhone == null)
                {
                    Phones newPhone = new Phones()
                    {
                        IsVerified = false,
                        Number = model.PhoneNumber,
                        UserId = Convert.ToInt32(model.UserId)
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
                int totalCount = 0;
                UsersGetResponse response = new UsersGetResponse();
                List<UsersModel> usersModelList = new List<UsersModel>();
                if (userId == "0")
                {
                    usersModelList = (from user in _context.Users
                                      join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                      join role in _context.Roles on usersRole.RoleId equals role.RoleId
                                      join phone in _context.Phones on user.UserId equals phone.UserId
                                      select new UsersModel
                                      {
                                          UserId = user.UserId.ToString(),
                                          Phone = phone.Number,
                                          Email = user.Email,
                                          CreatedAt = user.CreatedAt,
                                          Name = user.Name,
                                          Application = role.Application,
                                          Description = role.Description
                                      }).ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from user in _context.Users
                                  join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                  join role in _context.Roles on usersRole.RoleId equals role.RoleId
                                  join phone in _context.Phones on user.UserId equals phone.UserId
                                  select new UsersModel
                                  {
                                      UserId = user.UserId.ToString(),
                                      Phone = phone.Number,
                                      Email = user.Email,
                                      CreatedAt = user.CreatedAt,
                                      Name = user.Name,
                                      Application = role.Application,
                                      Description = role.Description
                                  }).ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().Count();
                }
                else
                {
                    usersModelList = (from user in _context.Users
                                      join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                      join role in _context.Roles on usersRole.RoleId equals role.RoleId
                                      join phone in _context.Phones on user.UserId equals phone.UserId
                                      where user.UserId == Convert.ToInt32(userId)
                                      select new UsersModel
                                      {
                                          UserId = user.UserId.ToString(),
                                          Phone = phone.Number,
                                          Email = user.Email,
                                          CreatedAt = user.CreatedAt,
                                          Name = user.Name,
                                          Application = role.Application,
                                          Description = role.Description
                                      }).ToList().GroupBy(p => p.UserId).Select(g => g.First()).OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from user in _context.Users
                                  join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                  join role in _context.Roles on usersRole.RoleId equals role.RoleId
                                  join phone in _context.Phones on user.UserId equals phone.UserId
                                  where user.UserId == Convert.ToInt32(userId)
                                  select new UsersModel
                                  {
                                      UserId = user.UserId.ToString(),
                                      Phone = phone.Number,
                                      Email = user.Email,
                                      CreatedAt = user.CreatedAt,
                                      Name = user.Name,
                                      Application = role.Application,
                                      Description = role.Description
                                  }).ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().Count();
                }

                if (usersModelList == null || usersModelList.Count == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status404NotFound);

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