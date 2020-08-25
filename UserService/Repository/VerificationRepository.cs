using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Verify.V2.Service;
using UserService.Abstraction;
using UserService.DatabaseAccess.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DBModels;

namespace UserService.Repository
{
    public class VerificationRepository : IVerificationRepository
    {
        private readonly Configuration.Twilio _config;

        public VerificationRepository(Configuration.Twilio configuration)
        {
            _config = configuration;
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);
        }

        public async Task<SignInV2Response> SendOTP(string phoneNumber)
        {
            SignInV2Response response = new SignInV2Response();
            try
            {
                var verificationResource = await VerificationResource.CreateAsync(
                    to: phoneNumber,
                    channel: "sms",
                    pathServiceSid: _config.VerificationSid
                );
                var result = new VerificationResult(verificationResource.Sid);

                if (!result.IsValid)
                {
                    response.status = false;
                    response.message = "Something went wrong while sending otp to your phone";
                    response.responseCode = ResponseCode.InternalServerError;
                    return response;
                }

                response.status = true;
                response.message = "An SMS with OTP is sent to your phone number successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (TwilioException e)
            {
                response.status = false;
                response.message = "Something went wrong while sending otp to your phone. Error Message - " + new VerificationResult(new List<string> { e.Message });
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public async Task<SignInV2Response> VerifyOTP(string phoneNumber, string code)
        {
            SignInV2Response response = new SignInV2Response();
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    response.status = false;
                    response.message = "Pass valid OTP code.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var verificationCheckResource = await VerificationCheckResource.CreateAsync(
                    to: phoneNumber,
                    code: code,
                    pathServiceSid: _config.VerificationSid
                );
                var result = verificationCheckResource.Status.Equals("approved") ?
                    new VerificationResult(verificationCheckResource.Sid) :
                    new VerificationResult(new List<string> { "Wrong code. Try again." });

                if (!result.IsValid)
                {
                    response.status = false;
                    response.message = "Something went wrong while verifying OTP. Error - " + result.Errors.ToString();
                    response.responseCode = ResponseCode.InternalServerError;
                    return response;
                }
                
                response.status = true;
                response.message = "OTP verified successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Invalid OTP.";
                response.responseCode = ResponseCode.BadRequest;
                return response;
            }
        }
    }
}
