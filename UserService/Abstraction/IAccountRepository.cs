using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;
using UserService.Models.DBModels;

namespace UserService.Abstraction
{
    public interface IAccountRepository
    {
        Task<dynamic> SignUp(RegistrationModel model);
        Task<(ErrorResponse errorResponse, SignInResponse response)> SignIn(SigninModel model, StringValues Application);
        Task<AuthenticationResponse> AuthenticateUser(SigninModel signinModel, StringValues application);
        Task<dynamic> ChangePassword(ChangePasswordModel model);
        Task<dynamic> ForgotPassword(string email);
    }
}
