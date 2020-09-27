using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IAccountRepository
    {
        Task<dynamic> SignUp(RegistrationModel model);
        Task<(ErrorResponse errorResponse, SignInResponse response)> SignIn(SigninModel model, StringValues Application);
        Task<dynamic> ChangePassword(ChangePasswordModel model);
        Task<dynamic> ForgotPassword(string email);
        Task<dynamic> QRSignin(SigninModel model, StringValues Application);
    }
}
