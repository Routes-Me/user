using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.Common;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api")]
    public class VerifyController : BaseController
    {
        private readonly IVerificationRepository _verification;
        private readonly IUserRepository _usersRepository;
        public VerifyController(IVerificationRepository verification, IUserRepository usersRepository)
        {
            _usersRepository = usersRepository;
            _verification = verification;
        }

        [HttpPost]
        [Route("sendverification")]
        public async Task<IActionResult> SendOTP(string phone, string channel)
        {
            SignInV2Response response = new SignInV2Response();
            if (ModelState.IsValid)
            {
                response = _usersRepository.CheckPhoneNumberExist(phone);
                if (response.responseCode != ResponseCode.Success)
                    return GetActionResult(response);

                response = await _verification.SendOTP(phone);
            }
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("verify")]
        public async Task<IActionResult> VerifyOTP(string phone, string code)
        {
            SignInV2Response response = new SignInV2Response();
            if (ModelState.IsValid)
            {
                response = _usersRepository.CheckPhoneNumberExist(phone);
                if (response.responseCode != ResponseCode.Success)
                    return GetActionResult(response);

                response = await _verification.VerifyOTP(phone, code);
                if (response.responseCode != ResponseCode.Success)
                    return GetActionResult(response);

                response = _usersRepository.ConfirmPhoneNumber(phone);
            }
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}