using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.DbModels
{
    public class IphoneDevices
    {
        [Key]
        public int IphoneDeviceId { get; set; }
        public string IosIdentifier { get; set; }
        [ForeignKey("Devices")]
        public int DeviceId { get; set; }
        public Devices Devices { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
