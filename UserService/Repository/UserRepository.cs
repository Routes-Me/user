﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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
        private readonly UsersServiceContext _context;
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;
        public UserRepository(IOptions<AppSettings> appSettings, UsersServiceContext context, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _dependencies = dependencies.Value;
        }

        public dynamic DeleteUser(string id)
        {
            try
            {
                var userIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(id), _appSettings.PrimeInverse);
                var users = _context.Users.Include(x => x.Phones).Where(x => x.UserId == userIdDecrypted).FirstOrDefault();
                if (users == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status404NotFound);

                _context.Users.Remove(users);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.UserDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateUser(UsersDto usersDto)
        {
            try
            {
                var userIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(usersDto.UserId), _appSettings.PrimeInverse);
                List<RolesModel> roles = new List<RolesModel>();
                if (usersDto == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.InvalidData, StatusCodes.Status400BadRequest);

                var user = _context.Users.Include(x => x.Phones).Where(x => x.UserId == userIdDecrypted).FirstOrDefault();
                if (user == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status404NotFound);

                if (!string.IsNullOrEmpty(usersDto.PhoneNumber))
                {
                    var userPhone = user.Phones.Where(x => x.UserId == userIdDecrypted).FirstOrDefault();
                    if (userPhone == null)
                    {
                        Phones newPhone = new Phones()
                        {
                            IsVerified = false,
                            Number = usersDto.PhoneNumber,
                            UserId = userIdDecrypted
                        };
                        _context.Phones.Add(newPhone);
                    }
                    else
                    {
                        userPhone.Number = usersDto.PhoneNumber;
                        userPhone.IsVerified = false;
                        _context.Phones.Update(userPhone);
                    }
                }

                user.Name = usersDto.Name;
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
                List<UsersDto> usersModelList = new List<UsersDto>();
                if (userIdDecrypted == 0)
                {
                    var usersData = _context.Users.Include(x => x.Phones).AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
                    foreach (var item in usersData)
                    {
                        UsersDto usersModel = new UsersDto();
                        usersModel.UserId = ObfuscationClass.EncodeId(item.UserId, _appSettings.Prime).ToString();
                        usersModel.PhoneNumber = item.Phones.Where(x => x.UserId == item.UserId).Select(x => x.Number).FirstOrDefault();
                        usersModel.CreatedAt = item.CreatedAt;
                        usersModel.Name = item.Name;
                        usersModelList.Add(usersModel);
                    }
                    totalCount = _context.Users.ToList().Count();
                }
                else
                {
                    var usersData = _context.Users.Include(x => x.Phones).Where(x => x.UserId == userIdDecrypted)
                        .AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    foreach (var item in usersData)
                    {
                        UsersDto usersModel = new UsersDto();
                        usersModel.UserId = ObfuscationClass.EncodeId(item.UserId, _appSettings.Prime).ToString();
                        usersModel.PhoneNumber = item.Phones.Where(x => x.UserId == item.UserId).Select(x => x.Number).FirstOrDefault();
                        usersModel.CreatedAt = item.CreatedAt;
                        usersModel.Name = item.Name;
                        usersModelList.Add(usersModel);
                    }
                    totalCount = _context.Users.Where(x => x.UserId == userIdDecrypted).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                dynamic includeData = new JObject();

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

        public dynamic PostUser(UsersDto usersDto)
        {
            if (usersDto == null)
                throw new ArgumentNullException(CommonMessage.InvalidData);

            if (_context.Phones.Where(p => p.Number == usersDto.PhoneNumber).FirstOrDefault() != null)
                throw new ArgumentException(CommonMessage.PhoneAlreadyExists);

            return new Users
            {
                Name = usersDto.Name,
                Phones = new List<Phones>
                {
                    new Phones { Number = usersDto.PhoneNumber, IsVerified = false }
                },
                CreatedAt = DateTime.Now,
                IsEmailVerified = false
            };
        }
    }
}