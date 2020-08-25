using System;
using System.Collections.Generic;

namespace UserService.Models.DBModels
{
    public partial class Users
    {
        public Users()
        {
            Phones = new HashSet<Phones>();
            UsersRoles = new HashSet<UsersRoles>();
        }

        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsEmailVerified { get; set; }

        public virtual ICollection<Phones> Phones { get; set; }
        public virtual ICollection<UsersRoles> UsersRoles { get; set; }
    }
}
