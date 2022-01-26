using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RoutesSecurity;
using System;
using UserService.Abstraction;
using UserService.Functions;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DbModels;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;
using UAParser;

namespace UserService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/")]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly UsersServiceContext _context;
        private readonly AppSettings _appSettings;
        public DeviceController(IDeviceRepository deviceRepository, UsersServiceContext context, IOptions<AppSettings> appSettings)
        {
            _deviceRepository = deviceRepository;
            _context = context;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        [Route("users/{userId}/devices")]
        public IActionResult PostDevice(DeviceDto deviceDto, string userId)
        {
            PostDeviceResponse response = new PostDeviceResponse();
            try
            {
                deviceDto.OS = Common.GetOs(HttpContext.Request.Headers["User-Agent"]).OS.Family.ToLower();
                deviceDto.UserId = Convert.ToString(Obfuscation.Decode(userId));
                Devices devices = _deviceRepository.PostDevice(deviceDto);
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
        [Route("users/{userId}/devices/{deviceId}")]
        public IActionResult UpdateFcmToken(DeviceDto deviceDto, string deviceId, string userId)
        {
            try
            {
                deviceDto.DeviceId = Convert.ToString(Obfuscation.Decode(deviceId));
                deviceDto.UserId = Convert.ToString(Obfuscation.Decode(userId));
                Response response = _deviceRepository.UpdateDevice(deviceDto);
                return StatusCode(response.statusCode, response);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + e.Message);
            }
        }

        [HttpDelete]
        [Route("users/{userId}/devices/{deviceId}")]
        public IActionResult DeleteDevice(string deviceId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                    throw new ArgumentNullException(CommonMessage.DeviceIdRequired);

                if (!_deviceRepository.AuthenticateUser(Obfuscation.Decode(userId)))
                    throw new ArgumentNullException(CommonMessage.UserNotFound);

                _deviceRepository.DeleteDevice(Obfuscation.Decode(deviceId));
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
        [Route("users/{phone}/{uniqueId}")]
        public IActionResult CheckDevice(string phone, string uniqueId)
        {
            VerificationHeader JwtHeader = Common.ParseJwtHeader(HttpContext.Request.Headers["verificationToken"]);
            ClientInfo ClientInfo = Common.GetOs(HttpContext.Request.Headers["User-Agent"]);

            try
            {
                if (String.Equals(JwtHeader.Subject, phone) && DateTime.Compare(JwtHeader.ExpiryTime, DateTime.UtcNow) > 0)
                {
                    if (!_deviceRepository.AuthenticateNumber(phone))
                        return NotFound();

                    if (_deviceRepository.DeviceExistance(uniqueId, ClientInfo.OS.Family.ToLower()))
                        return Ok();

                    return StatusCode(StatusCodes.Status404NotFound, CommonMessage.DeviceExist);
                }
                else
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, CommonMessage.InvalidToken);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
        }
    }
}
