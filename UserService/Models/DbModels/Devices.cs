using System;
using System.Collections.Generic;
using UserService.Models.DBModels;

namespace UserService.Models.DbModels
{
    public class Devices
    {
        public int DeviceId { get; set; }
        public string OS { get; set; }
        public int UserId { get; set; }
        public iphone_devices iphone_devices { get; set; }
        public android_devices android_devices { get; set; }
        public List<registration_notifications> registration_notifications { get; set; }
        public virtual Users User { get; set; }
        public DateTime created_at { get; set; }
    }
}
