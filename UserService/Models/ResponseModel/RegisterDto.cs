using System.Collections.Generic;

namespace UserService.Models.ResponseModel
{
    public class RegisterDto
    {
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<RolesModel> Roles { get; set; }
        public string Name { get; set; }
        public string InstitutionId { get; set; }
    }
}
