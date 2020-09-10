using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IAccountRepository
    {
        dynamic SignUp(RegistrationModel model);
        (ErrorResponse errorResponse, SignInResponse response) SignIn(SigninModel model);
        dynamic ChangePassword(ChangePasswordModel model);
        Task<dynamic> ForgotPassword(string email);
    }
}
