using System;
using System.Collections.Generic;

namespace UserService.Models.DBModels
{
    public partial class Usersavatar
    {
        public int UserAvatarId { get; set; }
        public int? UserId { get; set; }
        public string AvatarUrl { get; set; }

        public virtual Users User { get; set; }
    }
}
