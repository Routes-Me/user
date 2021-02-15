using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.ResponseModel;
using UserService.Models.DBModels;

namespace UserService.Controllers
{
    [Route("api")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IVerificationRepository _verificationRepository;
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly userserviceContext _context;
        public AccountController(IAccountRepository accountRepository, IVerificationRepository verificationRepository, userserviceContext context)
        {
            _accountRepository = accountRepository;
            _verificationRepository = verificationRepository;
            _context = context;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Signup(RegistrationModel model)
        {
            dynamic response = await _accountRepository.SignUp(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> Signin(SigninModel model)
        {
            StringValues Application;
            Request.Headers.TryGetValue("Application", out Application);
            dynamic response = await _accountRepository.SignIn(model, Application.FirstOrDefault());
            if (response.Item1 != null)
                return StatusCode((int)response.Item1.errors[0].statusCode, response.Item1);
            return StatusCode((int)response.Item2.statusCode, response.Item2);
        }

        [HttpPost]
        [Route("authentications")]
        public async Task<IActionResult> AuthenticateUser(SigninModel signinModel)
        {
            AuthenticationResponse authenticationResponse = new AuthenticationResponse();
            try
            {
                StringValues application;
                Request.Headers.TryGetValue("Application", out application);
                authenticationResponse = await _accountRepository.AuthenticateUser(signinModel, application.FirstOrDefault());
                _context.Users.Update(authenticationResponse.user);
                _context.SaveChanges();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (Exception ex)
            {
                dynamic errorResponse = ReturnResponse.ExceptionResponse(ex);
                return StatusCode((int)errorResponse.statusCode, errorResponse);
            }
            SignInResponse response = new SignInResponse();
            response.message = CommonMessage.LoginSuccess;
            response.status = true;
            response.token = authenticationResponse.accessToken;
            response.statusCode = StatusCodes.Status200OK;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            };
            Response.Cookies.Append("refreshToken", authenticationResponse.refreshToken, cookieOptions);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("authentications/renewals")]
        public async Task<IActionResult> RenewTokens(TokenRenewModel tokenRenewModel)
        {
            TokenRenewalResponse response = new TokenRenewalResponse();
            try
            {
                StringValues accessToken;
                Request.Headers.TryGetValue("AccessToken", out accessToken);
                response = _accountRepository.RenewTokens(tokenRenewModel.RefreshToken, accessToken);
            }
            catch (SecurityTokenExpiredException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                dynamic errorResponse = ReturnResponse.ExceptionResponse(ex);
                return StatusCode((int)errorResponse.statusCode, errorResponse);
            }
            response.message = CommonMessage.RenewSuccess;
            response.status = true;
            response.statusCode = StatusCodes.Status200OK;
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("account/password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            dynamic response = await _accountRepository.ChangePassword(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("account/password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            dynamic response = await _accountRepository.ForgotPassword(email);
            return StatusCode((int)response.statusCode, response);
        }
    }
}
