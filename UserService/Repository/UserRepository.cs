﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DbModels;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersServiceContext _context;
        private readonly AppSettings _appSettings;
        public UserRepository(IOptions<AppSettings> appSettings, UsersServiceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public dynamic DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(CommonMessage.UserIdRequired);

            int userIdDecrypted = Obfuscation.Decode(userId);
            Users user = _context.Users.Include(x => x.Phones).Where(x => x.UserId == userIdDecrypted).FirstOrDefault();
            if (user == null)
                throw new ArgumentException(CommonMessage.UserNotFound);

            return user;
        }

        public dynamic UpdateUser(UsersDto usersDto)
        {
            try
            {
                var userIdDecrypted = Obfuscation.Decode(usersDto.UserId);
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
                user.Email = usersDto.Email;
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
                int totalCount = 0;
                UsersGetResponse response = new UsersGetResponse();
                List<UsersDto> usersModelList = new List<UsersDto>();
                if (string.IsNullOrEmpty(userId))
                {
                    var usersData = _context.Users.Include(x => x.Phones).AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
                    foreach (var item in usersData)
                    {
                        UsersDto usersModel = new UsersDto();
                        usersModel.UserId = Obfuscation.Encode(item.UserId);
                        usersModel.PhoneNumber = item.Phones.Where(x => x.UserId == item.UserId).Select(x => x.Number).FirstOrDefault();
                        usersModel.CreatedAt = item.CreatedAt;
                        usersModel.Name = item.Name;
                        usersModel.Email = item.Email;
                        usersModelList.Add(usersModel);
                    }
                    totalCount = _context.Users.ToList().Count();
                }
                else
                {
                    var userIdDecrypted = Obfuscation.Decode(userId);
                    var usersData = _context.Users.Include(x => x.Phones).Where(x => x.UserId == userIdDecrypted)
                        .AsEnumerable().ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    foreach (var item in usersData)
                    {
                        UsersDto usersModel = new UsersDto();
                        usersModel.UserId = Obfuscation.Encode(item.UserId);
                        usersModel.PhoneNumber = item.Phones.Where(x => x.UserId == item.UserId).Select(x => x.Number).FirstOrDefault();
                        usersModel.CreatedAt = item.CreatedAt;
                        usersModel.Name = item.Name;
                        usersModel.Email = item.Email;
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

            if (!string.IsNullOrEmpty(usersDto.PhoneNumber) && _context.Phones.Where(p => p.Number == usersDto.PhoneNumber).FirstOrDefault() != null)
                throw new ArgumentException(CommonMessage.PhoneAlreadyExists);


            return new Users
            {
                Name = usersDto.Name,
                Email = usersDto.Email,
                Phones = new List<Phones>
                {
                    new Phones { Number = usersDto.PhoneNumber, IsVerified = false }
                },
                CreatedAt = DateTime.Now,
                IsEmailVerified = false
            };
        }

        public dynamic PostDevice(DeviceDto deviceDto)
        {
            if (deviceDto == null)
                throw new ArgumentNullException(CommonMessage.InvalidData);

            if(deviceDto.OS == "android" || deviceDto.OS == "Android")
            {
                if(_context.android_devices.Where(x => x.android_identifier == deviceDto.UniqueId).FirstOrDefault() != null)
                    throw new ArgumentException(CommonMessage.AndroidDeviceExist);

                return new Devices
                {
                    OS = deviceDto.OS,
                    UserId = Obfuscation.Decode(deviceDto.UserId),
                    android_devices = new android_devices
                    {
                        android_identifier = deviceDto.UniqueId,
                        created_at = DateTime.Now

                    },
                    created_at = DateTime.Now
                };
            }
            else
            {
                if (_context.iphone_devices.Where(x => x.ios_identifier == deviceDto.UniqueId).FirstOrDefault() != null)
                    throw new ArgumentException(CommonMessage.IphoneDeviceExist);

                return new Devices
                {
                    OS = deviceDto.OS,
                    UserId = Obfuscation.Decode(deviceDto.UserId),
                    iphone_devices = new iphone_devices
                    {
                        ios_identifier = deviceDto.UniqueId,
                        created_at = DateTime.Now

                    },
                    created_at = DateTime.Now
                };
            }
        }

        public dynamic UpdateDevice(DeviceDto deviceDto)
        {
            var DeviceIdDecrypted = Obfuscation.Decode(deviceDto.DeviceId);

            if (deviceDto == null)
                throw new ArgumentNullException(CommonMessage.InvalidData);

            var device = _context.registration_notifications.Where(x => x.DeviceId == DeviceIdDecrypted).FirstOrDefault();
            if (device == null)
            {
                registration_notifications reg = new registration_notifications
                {
                    FcmToken = deviceDto.FcmToken,
                    created_at = DateTime.Now,
                    DeviceId = DeviceIdDecrypted
                };

                _context.registration_notifications.Add(reg);
                _context.SaveChanges();

                return ReturnResponse.SuccessResponse(CommonMessage.FCMTokenPosted, false);
            }
            else
            {
                device.FcmToken = deviceDto.FcmToken;
                _context.registration_notifications.Update(device);
                _context.SaveChanges();

                return ReturnResponse.SuccessResponse(CommonMessage.FcmTokenUpdated, false);
            }
        }

        public dynamic DeleteDevice(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(CommonMessage.DeviceIdRequired);

            int deviceIdDecrypted = Obfuscation.Decode(deviceId);
            Devices dev = _context.Devices.Where(x => x.DeviceId == deviceIdDecrypted).FirstOrDefault();
            if (dev == null)
                throw new ArgumentException(CommonMessage.DeviceNotFound);

            return dev;
        }

    }
}