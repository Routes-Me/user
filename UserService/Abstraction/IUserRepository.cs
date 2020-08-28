using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IUserRepository
    {
        (ErrorResponse errorResponse, SignInResponse response) SignIn(SigninModel model);
        UsersResponse SignUp(RegistrationModel model);
        Task<SignInV2Response> SendSignInOTP(SignInOTPModel model);
        Task<SignInResponse> VerifySignInOTP(VerifySignInOTPModel model);
        UsersResponse DeleteUser(int id);
        UsersResponse UpdateUser(RegistrationModel model);
        UsersResponse ChangePassword(ChangePasswordModel model);
        UsersGetResponse GetUser(int userId, Pagination pageInfo);
        Task<EmailResponse> SendConfirmationEmail(EmailModel model);
        Task<EmailResponse> ForgotPassword(string email);
        EmailResponse VerifyEmail(int userId);
        SignInV2Response ConfirmPhoneNumber(string phone);
        SignInV2Response CheckPhoneNumberExist(string model);
    }
}
