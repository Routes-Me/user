using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.Common;

namespace UserService.Abstraction
{
    public interface IVerificationRepository
    {
        Task<SignInV2Response> SendOTP(string phoneNumber);

        Task<SignInV2Response> VerifyOTP(string phoneNumber, string code);
    }
}
