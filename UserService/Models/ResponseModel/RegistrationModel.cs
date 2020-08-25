using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models.ResponseModel
{
    public class RegistrationModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool? IsVerified { get; set; }
        public List<int> Roles { get; set; }
        public string Name { get; set; }
        public int InstitutionId { get; set; }
    }
}
