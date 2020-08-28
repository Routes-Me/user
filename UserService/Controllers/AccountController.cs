using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Controllers
{
    [Route("api")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IUserRepository _usersRepository;
        private static readonly HttpClient HttpClient = new HttpClient();
        public AccountController(IUserRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        [HttpPost]
        [Route("v1/{signin}")]
        public IActionResult Signin(SigninModel model)
        {
            var response = _usersRepository.SignIn(model);
            if (response.errorResponse != null)
                return GetErrorResult(response.errorResponse);
            return Ok(response.response);
        }

        [HttpPost]
        [Route("v2/signin")]
        public async Task<IActionResult> Signin(SignInOTPModel model)
        {
            SignInV2Response response = new SignInV2Response();
            response = await _usersRepository.SendSignInOTP(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("v2/{verifysignin}")]
        public async Task<IActionResult> VerifySigninOTP(VerifySignInOTPModel model)
        {
            SignInResponse response = new SignInResponse();
            response = await _usersRepository.VerifySignInOTP(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("signup")]
        public IActionResult Signup(RegistrationModel model)
        {
            UsersResponse response = new UsersResponse();
            response = _usersRepository.SignUp(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("v2/signup")]
        public IActionResult APISignup(APIRegistrationModel model)
        {
            UsersResponse response = new UsersResponse();
            if (ModelState.IsValid)
                 response = _usersRepository.APISignUp(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPut]
        [Route("changepassword")]
        public IActionResult ChangePassword(ChangePasswordModel model)
        {
            UsersResponse response = new UsersResponse();
            if (ModelState.IsValid)
                response = _usersRepository.ChangePassword(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        // Send confirmation email
        [HttpPost]
        [Route("email/{confirm}")]
        public async Task<IActionResult> SendConfirmationEmail(EmailModel model)
        {
            EmailResponse response = new EmailResponse();
            response = await _usersRepository.SendConfirmationEmail(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        // Verify email
        [AllowAnonymous]
        [HttpGet]
        [Route("email/{confirm}")]
        public IActionResult VerifyEmail(int id)
        {
            EmailResponse response = new EmailResponse();
            response = _usersRepository.VerifyEmail(id);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        // Send password to email
        [HttpPost]
        [Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            EmailResponse response = new EmailResponse();
            response = await _usersRepository.ForgotPassword(email);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}
