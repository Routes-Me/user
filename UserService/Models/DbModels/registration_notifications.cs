using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.DbModels
{
    public class registration_notifications
    { 
        [Key]
        public int RegisteredNotificationId { get; set; }
        [ForeignKey("Devices")]
        public int DeviceId { get; set; }
        public Devices Devices { get; set; }
        public string FcmToken { get; set; }
        public DateTime? created_at { get; set; }
    }
}
