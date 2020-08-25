using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models.ResponseModel
{
    public class UsersModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Application { get; set; }
        public string Description { get; set; }
    }
}
