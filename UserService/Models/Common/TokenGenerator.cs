using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models.ResponseModel;

namespace UserService.Models.Common
{
    public class TokenGenerator
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public List<UserRoleForToken> Roles { get; set; }
        public string InstitutionId { get; set; }
    }

}
