using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.ResponseModel;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api")]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationRepository _verificationRepository;
        public VerificationController(IVerificationRepository verificationRepository)
        {
            _verificationRepository = verificationRepository;
        }

        [HttpPost]
        [Route("otp")]
        public async Task<IActionResult> SendOTP(SendOTPModel model)
        {
            dynamic response = await _verificationRepository.SendOTP(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("otp/verify")]
        public async Task<IActionResult> VerifyOTP(VerifyOTPModel model)
        {
            dynamic response = await _verificationRepository.VerifyOTP(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("qr/otp")]
        public async Task<IActionResult> SendOTPForQRCode(SendOTPModel model)
        {
            dynamic response = await _verificationRepository.SendOTPForQRCode(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("qr/otp/verify")]
        public async Task<IActionResult> VerifyOTPForQRCode(VerifyOTPModel model)
        {
            dynamic response = await _verificationRepository.VerifyOTPForQRCode(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("signin/otp/verify")]
        public async Task<IActionResult> VerifySigninOTP(VerifyOTPModel model)
        {
            dynamic response = await _verificationRepository.VerifySigninOTP(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("qr/signin/otp/verify")]
        public async Task<IActionResult> QRVerifySigninOTP(VerifyOTPModel model)
        {
            dynamic response = await _verificationRepository.QRVerifySigninOTP(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("email")]
        public async Task<IActionResult> SendEmailConfirmation(EmailModel model)
        {
            dynamic response = await _verificationRepository.SendEmailConfirmation(model);
            return StatusCode((int)response.statusCode, response);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("email/verify")]
        public IActionResult VerifyEmailConfirmation(string id)
        {
            dynamic response = _verificationRepository.VerifyEmailConfirmation(id);
            return StatusCode((int)response.statusCode, response);
        }
    }
}