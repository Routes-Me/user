﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Models.ResponseModel
{
    public class UserRoleForToken
    {
        public string Application { get; set; }
        public string Privilege { get; set; }
    }
}
