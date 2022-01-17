using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.DbModels
{
    public class iphone_devices
    {
        [Key]
        public int iphone_device_id { get; set; }
        public string ios_identifier { get; set; }
        [ForeignKey("Devices")]
        public int DeviceId { get; set; }
        public Devices Devices { get; set; }
        public DateTime? created_at { get; set; }
    }
}
