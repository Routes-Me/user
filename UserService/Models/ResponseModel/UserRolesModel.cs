using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models.ResponseModel
{
    public class RolesModel
    {
        public string RoleId { get; set; }
        public string Application { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
