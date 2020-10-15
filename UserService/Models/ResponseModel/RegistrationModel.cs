﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models.ResponseModel
{
    public class RegistrationModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool? IsVerified { get; set; }
        public List<RolesModel> Roles { get; set; }
        public string Name { get; set; }
        public string InstitutionId { get; set; }
    }

    public class QRCodeRegistrationModel
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<int> Roles { get; set; }
    }

    public class privilege
    {
        public string ApplicationId { get; set; }
        public string PrivilegeId { get; set; }
    }
}
