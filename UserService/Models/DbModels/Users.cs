 using System;
using System.Collections.Generic;
using UserService.Models.DbModels;

namespace UserService.Models.DBModels
{
    public partial class Users
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsEmailVerified { get; set; }

        public virtual ICollection<Phones> Phones { get; set; }
        public virtual ICollection<Devices> Devices { get; set; }
    }
}
