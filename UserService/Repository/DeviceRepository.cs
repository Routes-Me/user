using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DbModels;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly UsersServiceContext _context;
        private readonly AppSettings _appSettings;
        public DeviceRepository(IOptions<AppSettings> appSettings, UsersServiceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public Devices PostDevice(DeviceDto deviceDto)
        {
            if (deviceDto == null)
                throw new ArgumentNullException(CommonMessage.InvalidData);

            if (_context.Devices.Where(x => x.AndroidDevices.AndroidIdentifier == deviceDto.UniqueId || x.IphoneDevices.IosIdentifier == deviceDto.UniqueId).FirstOrDefault() != null)
                throw new ArgumentException(CommonMessage.DeviceExist);

            Devices device = new Devices
            {
                UserId = Convert.ToInt32(deviceDto.UserId),
                CreatedAt = DateTime.Now,
            };

            if (deviceDto.OS == OsTypes.android.ToString())
            {
                device.OS = OsTypes.android;
                device.AndroidDevices = new AndroidDevices()
                {
                    AndroidIdentifier = deviceDto.UniqueId,
                    CreatedAt = DateTime.Now,
                };
            }

            if (deviceDto.OS == OsTypes.ios.ToString())
            {
                device.OS = OsTypes.ios;
                device.IphoneDevices = new IphoneDevices()
                {
                    IosIdentifier = deviceDto.UniqueId,
                    CreatedAt = DateTime.Now,
                };
            }

            _context.Devices.Add(device);
            _context.SaveChanges();

            return device;
        }

        public Response UpdateDevice(DeviceDto deviceDto)
        {

            if (deviceDto == null)
                throw new ArgumentNullException(CommonMessage.InvalidData);

            if (_context.Users.Where(x => x.UserId == Convert.ToInt32(deviceDto.UserId)).FirstOrDefault() == null)
                throw new NullReferenceException(CommonMessage.UserNotFound);

            if (_context.Devices.Where(x => x.DeviceId == Convert.ToInt32(deviceDto.DeviceId)).FirstOrDefault() == null)
                throw new NullReferenceException(CommonMessage.DeviceNotFound);

            var device = _context.RegistrationNotifications.Where(x => x.DeviceId == Convert.ToInt32(deviceDto.DeviceId)).FirstOrDefault();
            if (device == null)
            {
                RegistrationNotifications reg = new RegistrationNotifications
                {
                    FcmToken = deviceDto.FcmToken,
                    CreatedAt = DateTime.Now,
                    DeviceId = Convert.ToInt32(deviceDto.DeviceId)
                };

                _context.RegistrationNotifications.Add(reg);
                _context.SaveChanges();

                return ReturnResponse.SuccessResponse(CommonMessage.FCMTokenPosted, false);
            }
            else
            {
                device.FcmToken = deviceDto.FcmToken;
                _context.RegistrationNotifications.Update(device);
                _context.SaveChanges();

                return ReturnResponse.SuccessResponse(CommonMessage.FcmTokenUpdated, false);
            }
        }

        public void DeleteDevice(int deviceId)
        {
            Devices dev = _context.Devices.Where(x => x.DeviceId == deviceId).FirstOrDefault();

            if (dev == null)
                throw new ArgumentException(CommonMessage.DeviceNotFound);

            _context.Devices.Remove(dev);
            _context.SaveChanges();
        }

        public bool DeviceExistance(string uniqueId, string os)
        {
            if (String.Equals(os, OsTypes.android.ToString()))
            {
                if (_context.Users.Include("Devices").Any(x => x.Devices.Any(x => x.AndroidDevices.AndroidIdentifier == uniqueId)))
                {
                    return true;
                }
            }

            if (String.Equals(os, OsTypes.ios.ToString()))
            {
                if (_context.Users.Include("Devices").Any(x => x.Devices.Any(x => x.IphoneDevices.IosIdentifier == uniqueId)))
                {
                    return true;
                }
            }

            return false;

        }

        public bool AuthenticateNumber(string number)
        {
            if (_context.Users.Include("Phones").Any(x => x.Phones.Any(x => x.Number == number)))
                return true;

            return false;
        }

        public bool AuthenticateUser(int userId)
        {
            if (_context.Users.Any(x => x.UserId == userId))
                return true;

            return false;
        }

    }
}
