using System;
using System.Collections.Generic;
using UserService.Models.DBModels;

namespace UserService.Models.DbModels
{
    public class Devices
    {
        public int DeviceId { get; set; }
        public OsTypes? OS { get; set; }
        public int UserId { get; set; }
        public IphoneDevices IphoneDevices { get; set; }
        public AndroidDevices AndroidDevices { get; set; }
        public List<RegistrationNotifications> RegistrationNotifications { get; set; }
        public virtual Users User { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public enum OsTypes
    {
        android ,
        ios 
    }
}
