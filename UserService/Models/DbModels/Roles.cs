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

        public int ApplicationId { get; set; }
        public int PrivilegeId { get; set; }

        public virtual Applications Application { get; set; }
        public virtual Privileges Privilege { get; set; }
        public virtual ICollection<UsersRoles> UsersRoles { get; set; }
    }
}
