using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.DbModels
{
    public class RegistrationNotifications
    { 
        [Key]
        public int RegisteredNotificationId { get; set; }
        [ForeignKey("Devices")]
        public int DeviceId { get; set; }
        public Devices Devices { get; set; }
        public string FcmToken { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
