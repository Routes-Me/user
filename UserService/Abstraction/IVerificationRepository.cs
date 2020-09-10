﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IVerificationRepository
    {
        Task<dynamic> SendOTP(SendOTPModel model);

        Task<dynamic> VerifyOTP(VerifyOTPModel model);

        Task<dynamic> VerifySigninOTP(VerifyOTPModel model);
        Task<dynamic> SendEmailConfirmation(EmailModel model);
        dynamic VerifyEmailConfirmation(int id);
    }
}
