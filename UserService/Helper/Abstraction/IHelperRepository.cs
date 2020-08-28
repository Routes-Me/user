﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models.Common;
using UserService.Models.ResponseModel;

namespace UserService.Helper.Abstraction
{
    public interface IHelperRepository
    {
        string GenerateToken(TokenGenerator user);
        Task<SendGrid.Response> SendConfirmationEmail(int userId, string email, string siteUrl);
        Task<SendGrid.Response> VerifyEmail(string email, string password);
    }
}