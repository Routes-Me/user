﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RoutesSecurity;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.DBModels;
using UserService.Models.Common;
using UserService.Models.ResponseModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserService.Models.DbModels;

namespace UserService.Controllers
{
    [ApiController]
    [ApiVersion( "1.0" )]
    [Route("v{version:apiVersion}/")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _usersRepository;
        private readonly UsersServiceContext _context;
        private readonly AppSettings _appSettings;
        public UserController(IUserRepository usersRepository, UsersServiceContext context, IOptions<AppSettings> appSettings)
        {
            _usersRepository = usersRepository;
            _context = context;
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        [Route("users/{id?}")]
        public IActionResult Get(string id, string Include, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _usersRepository.GetUser(id, pageInfo, Include);
            return StatusCode(response.statusCode, response);
        }

        [HttpDelete]
        [Route("users/{userId}")]
        public IActionResult delete(string userId)
        {
            try
            {
                Users user = _usersRepository.DeleteUser(userId);
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpPut]
        [Route("users")]
        public IActionResult Put(UsersDto usersDto)
        {
            dynamic response = _usersRepository.UpdateUser(usersDto);
            return StatusCode(response.statusCode, response);
        }

        [HttpPost]
        [Route("users")]
        public async Task<IActionResult> PostUser(UsersDto usersDto)
        {
            PostUserResponse response = new PostUserResponse();
            try
            {
                Users user = _usersRepository.PostUser(usersDto);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                response.UserId = Obfuscation.Encode(user.UserId);
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            response.Message = CommonMessage.UserInsert;
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost]
        [Route("users/devices")]
        public IActionResult PostDevice(DeviceDto deviceDto)
        {
            PostDeviceResponse response = new PostDeviceResponse();
            try
            {
                Devices devices = _usersRepository.PostDevice(deviceDto);
                response.DeviceId = Obfuscation.Encode(devices.DeviceId);
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            response.Message = CommonMessage.DevicePosted;
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut]
        [Route("users/updatedevices/{deviceId}")]
        public IActionResult UpdateFcmToken(DeviceDto deviceDto , string deviceId)
        {
            try
            {
                deviceDto.DeviceId = deviceId;
                dynamic response = _usersRepository.UpdateDevice(deviceDto);
                return StatusCode(response.statusCode, response);
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + e.Message);
            }
        }

        [HttpDelete]
        [Route("users/devices/{deviceId}")]
        public IActionResult DeleteDevice(string deviceId)
        {
            try
            {
                _usersRepository.DeleteDevice(deviceId);
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet]
        [Route("users/{number}/{uniqueid}/{os}")]
        public IActionResult VerifyNumber(string number, string uniqueid, string OS)
        {
            if (string.Equals(OS.ToLower() , OsTypes.android.ToString()))
            {
                if (_usersRepository.DeviceExistance(number, uniqueid, "android"))
                {
                    return Ok();
                }
            }

            if (string.Equals(OS, OsTypes.ios.ToString()))
            {
                if (_usersRepository.DeviceExistance(number , uniqueid , "ios"))
                {
                    return Ok();
                }
            }

            return NotFound();
        }

    }
}