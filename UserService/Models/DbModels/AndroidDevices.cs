using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.DbModels
{
    public class AndroidDevices
    {
        [Key]
        public int AndroidDeviceId { get; set; }
        public string AndroidIdentifier { get; set; }
        [ForeignKey("Devices")]
        public int DeviceId { get; set; }
        public Devices Devices { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
