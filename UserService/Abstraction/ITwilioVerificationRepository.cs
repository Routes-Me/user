﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Abstraction
{
    public interface ITwilioVerificationRepository
    {
        Task<bool> TwilioVerificationResource(string phone);
        Task<bool> TwilioVerificationCheckResource(string phone, string code);
    }
}
