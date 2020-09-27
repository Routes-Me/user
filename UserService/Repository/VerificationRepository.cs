using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Verify.V2.Service;
using UserService.Abstraction;
using UserService.Helper.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class VerificationRepository : IVerificationRepository
    {
        private readonly userserviceContext _context;
        private readonly IHelperRepository _helper;
        private readonly ITwilioVerificationRepository _twilioVerificationRepository;

        public VerificationRepository(userserviceContext context, IHelperRepository helper, ITwilioVerificationRepository twilioVerificationRepository)
        {
            _context = context;
            _helper = helper;
            _twilioVerificationRepository = twilioVerificationRepository;
        }

        public async Task<dynamic> SendOTP(SendOTPModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Phone))
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneRequired, StatusCodes.Status400BadRequest);

                var phone = _context.Phones.Where(x => x.Number == model.Phone).FirstOrDefault();
                if (phone == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneNotExist, StatusCodes.Status404NotFound);

                bool result = await _twilioVerificationRepository.TwilioVerificationResource(model.Phone);
                if (!result)
                    return ReturnResponse.ErrorResponse(CommonMessage.OtpSendFailed, StatusCodes.Status500InternalServerError);

                return ReturnResponse.SuccessResponse(CommonMessage.OtpSendSuccess, false);
            }
            catch (TwilioException ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> VerifyOTP(VerifyOTPModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Phone))
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneRequired, StatusCodes.Status400BadRequest);

                if (string.IsNullOrEmpty(model.Code))
                    return ReturnResponse.ErrorResponse(CommonMessage.OtpNotFound, StatusCodes.Status400BadRequest);

                var phone = _context.Phones.Where(x => x.Number == model.Phone).FirstOrDefault();
                if (phone == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneNotExist, StatusCodes.Status404NotFound);

                bool result = await _twilioVerificationRepository.TwilioVerificationCheckResource(model.Phone, model.Code);
                if (!result)
                    return ReturnResponse.ErrorResponse(CommonMessage.OtpInvalid, StatusCodes.Status401Unauthorized);

                phone.IsVerified = true;
                _context.Phones.Update(phone);
                _context.SaveChanges();

                return ReturnResponse.SuccessResponse(CommonMessage.OtpVerifiedSuccess, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> SendOTPForQRCode(SendOTPModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Phone))
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneRequired, StatusCodes.Status400BadRequest);

                bool result = await _twilioVerificationRepository.TwilioVerificationResource(model.Phone);
                if (!result)
                    return ReturnResponse.ErrorResponse(CommonMessage.OtpSendFailed, StatusCodes.Status500InternalServerError);

                return ReturnResponse.SuccessResponse(CommonMessage.OtpSendSuccess, false);
            }
            catch (TwilioException ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> VerifyOTPForQRCode(VerifyOTPModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Phone))
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneRequired, StatusCodes.Status400BadRequest);

                if (string.IsNullOrEmpty(model.Code))
                    return ReturnResponse.ErrorResponse(CommonMessage.OtpNotFound, StatusCodes.Status400BadRequest);

                bool result = await _twilioVerificationRepository.TwilioVerificationCheckResource(model.Phone, model.Code);
                if (!result)
                    return ReturnResponse.ErrorResponse(CommonMessage.OtpInvalid, StatusCodes.Status401Unauthorized);

                return ReturnResponse.SuccessResponse(CommonMessage.OtpVerifiedSuccess, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> VerifySigninOTP(VerifyOTPModel model, StringValues Application)
        {
            try
            {
                SignInResponse response = new SignInResponse();
                if (string.IsNullOrEmpty(model.Phone))
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneRequired, StatusCodes.Status400BadRequest);

                if (string.IsNullOrEmpty(model.Code))
                    return ReturnResponse.ErrorResponse(CommonMessage.OtpRequired, StatusCodes.Status400BadRequest);

                var phone = _context.Phones.Include(x => x.User).Include(x => x.User.UsersRoles).Where(x => x.Number == model.Phone).FirstOrDefault();
                if (phone == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneNotExist, StatusCodes.Status404NotFound);

                if (phone.User == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotExist, StatusCodes.Status404NotFound);

                if (phone.User.UsersRoles == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotAssociatedWithUserRole, StatusCodes.Status404NotFound);

                TokenGenerator tokenGenerator = new TokenGenerator();
                foreach (var item in phone.User.UsersRoles)
                {
                    var role = _context.Roles.Where(x => x.RoleId == item.RoleId).FirstOrDefault();
                    if (role == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.UserRoleNotFound, StatusCodes.Status404NotFound);

                    tokenGenerator.RoleName = role.Privilege;
                }
                tokenGenerator.UserId = phone.User.UserId;
                tokenGenerator.Email = phone.User.Email;
                var result = await VerifyOTP(model);
                if (result.statusCode != StatusCodes.Status200OK)
                    return ReturnResponse.ErrorResponse(result.message, result.statusCode);

                string Token = _helper.GenerateToken(tokenGenerator, Application);
                response.message = CommonMessage.LoginSuccess;
                response.status = true;
                response.token = Token;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> SendEmailConfirmation(EmailModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                    return ReturnResponse.ErrorResponse(CommonMessage.EmailRequired, StatusCodes.Status400BadRequest);

                if (string.IsNullOrEmpty(model.RedirectUrl))
                    return ReturnResponse.ErrorResponse(CommonMessage.RedirectUrlRequired, StatusCodes.Status400BadRequest);

                var userData = _context.Users.Where(x => x.UserId == Convert.ToInt32(model.UserId)).FirstOrDefault();
                if (userData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status404NotFound);

                if (userData.Email.ToLower() != model.Email.ToLower())
                    return ReturnResponse.ErrorResponse(CommonMessage.EmailNotBelongToUser, StatusCodes.Status404NotFound);

                var res = await _helper.SendConfirmationEmail(Convert.ToInt32(model.UserId), model.Email, model.RedirectUrl);
                if (res.StatusCode != HttpStatusCode.Accepted)
                    return ReturnResponse.ErrorResponse(CommonMessage.EmailVerificationNotSend, StatusCodes.Status500InternalServerError);

                return ReturnResponse.SuccessResponse(CommonMessage.EmailVerificationSendSuccess, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic VerifyEmailConfirmation(string id)
        {
            try
            {
                var usersData = _context.Users.Where(x => x.UserId == Convert.ToInt32(id)).FirstOrDefault();
                if (usersData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status400BadRequest);

                usersData.IsEmailVerified = true;
                _context.Users.Update(usersData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.EmailVerificationSuccess, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> QRVerifySigninOTP(VerifyOTPModel model, StringValues Application)
        {
            try
            {
                QrSignInResponse response = new QrSignInResponse();
                if (string.IsNullOrEmpty(model.Phone))
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneRequired, StatusCodes.Status400BadRequest);

                if (string.IsNullOrEmpty(model.Code))
                    return ReturnResponse.ErrorResponse(CommonMessage.OtpRequired, StatusCodes.Status400BadRequest);

                var phone = _context.Phones.Include(x => x.User).Include(x => x.User.UsersRoles).Where(x => x.Number == model.Phone).FirstOrDefault();
                if (phone == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.PhoneNotExist, StatusCodes.Status404NotFound);

                if (phone.User == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotExist, StatusCodes.Status404NotFound);

                if (phone.User.UsersRoles == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserNotAssociatedWithUserRole, StatusCodes.Status404NotFound);

                TokenGenerator tokenGenerator = new TokenGenerator();
                foreach (var item in phone.User.UsersRoles)
                {
                    var role = _context.Roles.Where(x => x.RoleId == item.RoleId).FirstOrDefault();
                    if (role == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.UserRoleNotFound, StatusCodes.Status404NotFound);

                    tokenGenerator.RoleName = role.Privilege;
                }
                tokenGenerator.UserId = phone.User.UserId;
                tokenGenerator.Email = phone.User.Email;
                var result = await VerifyOTP(model);
                if (result.statusCode != StatusCodes.Status200OK)
                    return ReturnResponse.ErrorResponse(result.message, result.statusCode);

                string token = _helper.GenerateToken(tokenGenerator, Application);
                LoginUser loginUser = new LoginUser()
                {
                    UserId = Convert.ToString(phone.User.UserId),
                    Name = phone.User.Name,
                    Email = phone.User.Email,
                    Phone = phone.Number,
                    Token = token,
                };
                response.message = CommonMessage.LoginSuccess;
                response.status = true;
                response.user = loginUser;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}
