using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models
{
    public class EmailModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}
