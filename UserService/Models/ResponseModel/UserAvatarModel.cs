using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models.ResponseModel
{
    public class UsersAvatarModel
    {
        public int UserAvatarId { get; set; }
        public int UserId { get; set; }
        public string AvatarUrl { get; set; }
        public IFormFile Avatar { get; set; }
    }
}
