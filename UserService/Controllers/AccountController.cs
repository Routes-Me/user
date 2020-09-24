﻿using System;
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
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IVerificationRepository _verificationRepository;
        private static readonly HttpClient HttpClient = new HttpClient();
        public AccountController(IAccountRepository accountRepository, IVerificationRepository verificationRepository)
        {
            _accountRepository = accountRepository;
            _verificationRepository = verificationRepository;
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
            dynamic response = await _accountRepository.SignIn(model);
            if (response.Item1 != null)
                return StatusCode((int)response.Item1.errors[0].statusCode, response.Item1.errors);
            return StatusCode((int)response.Item2.statusCode, response.Item2);
        }

        [HttpPost]
        [Route("qr/signin")]
        public async Task<IActionResult> QRSignin(SigninModel model)
        {
            dynamic response = await _accountRepository.QRSignin(model);
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
