using System.Collections.Generic;
using UserService.Models.ResponseModel;

namespace UserService.Models.Common
{
    public class AccessTokenGenerator
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public List<UserRoleForToken> Roles { get; set; }
        public string InstitutionId { get; set; }
    }

}
