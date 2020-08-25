using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models
{
    public class VerifySignInOTPModel
    {
        public string Phone { get; set; }
        public string Code { get; set; }
    }
}
