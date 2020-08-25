using System;
using System.Collections.Generic;

namespace UserService.Models.DBModels
{
    public partial class UsersRoles
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }

        public virtual Roles Role { get; set; }
        public virtual Users User { get; set; }
    }
}
