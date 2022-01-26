using UserService.Models;
using UserService.Models.DbModels;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IDeviceRepository
    {
        Devices PostDevice(DeviceDto deviceDto);
        Response UpdateDevice(DeviceDto deviceDto);
        void DeleteDevice(int deviceId);
        bool DeviceExistance(string uniqueid, string Os);
        bool AuthenticateNumber(string number);
        bool AuthenticateUser(int userId);
    }
}
