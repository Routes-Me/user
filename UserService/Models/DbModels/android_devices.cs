using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.DbModels
{
    public class android_devices
    {
        [Key]
        public int android_device_id { get; set; }
        public string android_identifier { get; set; }
        [ForeignKey("Devices")]
        public int DeviceId { get; set; }
        public Devices Devices { get; set; }
        public DateTime? created_at { get; set; }
    }
}
