using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models.Common
{
    public class TokenGenerator
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public int UserRoleId { get; set; }
    }
}
