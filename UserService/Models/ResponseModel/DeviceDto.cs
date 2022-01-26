using UserService.Models.DbModels;

namespace UserService.Models.ResponseModel
{
    public class DeviceDto
    {
        public string DeviceId { get; set; }
        public string UniqueId { get; set; }
        public string FcmToken { get; set; }
        public string OS { get; set; }
        public string UserId { get; set; }
    }

}
