﻿using System;
using System.Collections.Generic;

namespace UserService.Models.DBModels
{
    public partial class Applications
    {
        public Applications()
        {
            Roles = new HashSet<Roles>();
        }

        public int ApplicationId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Roles> Roles { get; set; }
    }
}
