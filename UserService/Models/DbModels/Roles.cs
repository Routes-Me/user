using System;
using System.Collections.Generic;

namespace UserService.Models.DBModels
{
    public partial class Roles
    {
        public Roles()
        {
            UsersRoles = new HashSet<UsersRoles>();
        }

        public int RoleId { get; set; }
        public string Application { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<UsersRoles> UsersRoles { get; set; }
    }
}
