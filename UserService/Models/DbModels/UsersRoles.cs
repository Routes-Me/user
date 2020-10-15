using System;
using System.Collections.Generic;

namespace UserService.Models.DBModels
{
    public partial class UsersRoles
    {
        public int UserId { get; set; }
        public int ApplicationId { get; set; }
        public int PrivilegeId { get; set; }

        public virtual Users User { get; set; }
    }
}
