using Encryption;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Obfuscation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UserService.Abstraction;
using UserService.Helper.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly userserviceContext _context;
        private readonly IHelperRepository _helper;
        private readonly IPasswordHasherRepository _passwordHasherRepository;
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;
        EncryptionClass encryption = new EncryptionClass();

        public AccountRepository(IOptions<AppSettings> appSettings, userserviceContext context, IHelperRepository helper, IPasswordHasherRepository passwordHasherRepository, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _helper = helper;
            _passwordHasherRepository = passwordHasherRepository;
            _dependencies = dependencies.Value;
        }

        public async Task<dynamic> SignUp(RegistrationModel model)
        {
            UsersResponse response = new UsersResponse();
            List<RolesModel> roles = new List<RolesModel>();
            try
            {
                int institutionIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.InstitutionId), _appSettings.PrimeInverse);
                string originalPassword = string.Empty;
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                if (model.Roles.Count == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.UserRoleRequired, StatusCodes.Status400BadRequest);

                foreach (var role in model.Roles)
                {
                    var userRole = _context.Roles.Where(x => x.ApplicationId == ObfuscationClass.DecodeId(Convert.ToInt32(role.ApplicationId), _appSettings.PrimeInverse)
                    && x.PrivilegeId == ObfuscationClass.DecodeId(Convert.ToInt32(role.PrivilegeId), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (userRole == null)
                    {
                        return ReturnResponse.ErrorResponse(CommonMessage.UserRoleNotFound, StatusCodes.Status404NotFound);
                    }
                    else
                    {
                        RolesModel rolesModel = new RolesModel();
                        rolesModel.ApplicationId = ObfuscationClass.EncodeId(userRole.ApplicationId, _appSettings.Prime).ToString();
                        rolesModel.PrivilegeId = ObfuscationClass.EncodeId(userRole.PrivilegeId, _appSettings.Prime).ToString();
                        roles.Add(rolesModel);
                    }
                }

                if (string.IsNullOrEmpty(model.PhoneNumber) && string.IsNullOrEmpty(model.Email))
                    return ReturnResponse.ErrorResponse(CommonMessage.EmailPhoneRequired, StatusCodes.Status400BadRequest);

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    var phones = _context.Phones.Where(x => x.Number == model.PhoneNumber).FirstOrDefault();
                    if (phones != null)
                        return ReturnResponse.ErrorResponse(CommonMessage.PhoneExist, StatusCodes.Status409Conflict);
                }

                if (!string.IsNullOrEmpty(model.Email))
                {
                    var userEmail = _context.Users.Where(x => x.Email == model.Email).FirstOrDefault();
                    if (userEmail != null)
                        return ReturnResponse.ErrorResponse(CommonMessage.EmailExist, StatusCodes.Status409Conflict);
                }

                if (!string.IsNullOrEmpty(model.Password))
                {
                    originalPassword = await PasswordDecryptionAsync(model.Password);
                    if (originalPassword == "Unauthorized Access")
                        return ReturnResponse.ErrorResponse(CommonMessage.IncorrectPassword, StatusCodes.Status400BadRequest);
                }

                Users users = new Users()
                {
                    Name = model.Name,
                    Email = model.Email,
                    CreatedAt = DateTime.Now,
                    Password = string.IsNullOrEmpty(originalPassword) ? null : _passwordHasherRepository.Hash(originalPassword),
                    IsEmailVerified = false
                };
                _context.Users.Add(users);
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(model.PhoneNumber))
                {
                    Phones phone = new Phones()
                    {
                        UserId = users.UserId,
                        IsVerified = false,
                        Number = model.PhoneNumber,
                    };
                    _context.Phones.Add(phone);
                    _context.SaveChanges();
                }

                foreach (var role in roles)
                {
                    UsersRoles usersroles = new UsersRoles()
                    {
                        UserId = users.UserId,
                        ApplicationId = ObfuscationClass.DecodeId(Convert.ToInt32(role.ApplicationId), _appSettings.PrimeInverse),
                        PrivilegeId = ObfuscationClass.DecodeId(Convert.ToInt32(role.PrivilegeId), _appSettings.PrimeInverse)
                    };
                    _context.UsersRoles.Add(usersroles);
                }
                _context.SaveChanges();

                if (institutionIdDecrypted != 0)
                {
                    DriversModel driver = new DriversModel()
                    {
                        InstitutionId = model.InstitutionId,
                        UserId = ObfuscationClass.EncodeId(users.UserId, _appSettings.Prime).ToString()
                    };

                    var client = new RestClient(_appSettings.Host + _dependencies.VehicleUrl);
                    var request = new RestRequest(Method.POST);
                    string jsonToSend = JsonConvert.SerializeObject(driver);
                    request.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                    request.RequestFormat = DataFormat.Json;
                    IRestResponse institutionResponse = client.Execute(request);
                    if (institutionResponse.StatusCode != HttpStatusCode.Created)
                    {
                        response.status = false;
                        response.message = CommonMessage.UnableToInsertUserIntoDriver + institutionResponse.Content;
                        response.statusCode = StatusCodes.Status500InternalServerError;
                        return response;
                    }
                }
                response.status = true;
                response.message = CommonMessage.UserInsert;
                response.statusCode = StatusCodes.Status201Created;
                response.Email = model.Email;
                response.UserId = ObfuscationClass.EncodeId(users.UserId, _appSettings.Prime).ToString();
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<(ErrorResponse errorResponse, SignInResponse response)> SignIn(SigninModel model, StringValues Application)
        {
            SignInResponse response = new SignInResponse();
            ErrorResponse errorResponse = new ErrorResponse();
            errorResponse.errors = new List<ErrorDetails>();
            ErrorDetails errorDetails = new ErrorDetails();
            string originalPassword = string.Empty;
            try
            {
                Users user = new Users();
                user = _context.Users.Where(x => x.Email == model.Username).FirstOrDefault();
                if (user == null)
                {
                    var phoneUser = _context.Phones.Where(x => x.Number == model.Username).FirstOrDefault();
                    if (phoneUser == null)
                    {
                        errorDetails.statusCode = StatusCodes.Status401Unauthorized;
                        errorDetails.code = 1;
                        errorDetails.detail = CommonMessage.IncorrectUser;
                        errorResponse.errors.Add(errorDetails);
                        return (errorResponse, null);
                    }

                    user = _context.Users.Where(x => x.UserId == phoneUser.UserId).FirstOrDefault();
                    if (user == null)
                    {
                        errorDetails.statusCode = StatusCodes.Status401Unauthorized;
                        errorDetails.code = 1;
                        errorDetails.detail = CommonMessage.IncorrectUser;
                        errorResponse.errors.Add(errorDetails);
                        return (errorResponse, null);
                    }

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        originalPassword = await PasswordDecryptionAsync(model.Password);
                        if (originalPassword == "Unauthorized Access")
                            return ReturnResponse.ErrorResponse(CommonMessage.IncorrectPassword, StatusCodes.Status400BadRequest);
                    }

                    var isVerified = _passwordHasherRepository.Check(user.Password, originalPassword).Verified;
                    if (!isVerified)
                    {
                        errorDetails.statusCode = StatusCodes.Status401Unauthorized;
                        errorDetails.code = 2;
                        errorDetails.detail = CommonMessage.IncorrectPassword;
                        errorResponse.errors.Add(errorDetails);
                        return (errorResponse, null);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        originalPassword = await PasswordDecryptionAsync(model.Password);
                        if (originalPassword == "Unauthorized Access")
                            return ReturnResponse.ErrorResponse(CommonMessage.IncorrectPassword, StatusCodes.Status400BadRequest);
                    }

                    var isVerified = _passwordHasherRepository.Check(user.Password, originalPassword).Verified;
                    if (!isVerified)
                    {
                        errorDetails.statusCode = StatusCodes.Status401Unauthorized;
                        errorDetails.code = 2;
                        errorDetails.detail = CommonMessage.IncorrectPassword;
                        errorResponse.errors.Add(errorDetails);
                        return (errorResponse, null);
                    }
                }

                var usersRoles = (from usersrole in _context.UsersRoles
                                  join roles in _context.Roles on new { x = usersrole.PrivilegeId, y = usersrole.ApplicationId } equals new { x = roles.PrivilegeId, y = roles.ApplicationId }
                                  where usersrole.UserId == user.UserId
                                  select new UserRoleForToken
                                  {
                                      Application = roles.Application.Name,
                                      Privilege = roles.Privilege.Name,
                                  }).ToList();

                if (usersRoles == null || usersRoles.Count == 0)
                {
                    errorDetails.statusCode = StatusCodes.Status404NotFound;
                    errorDetails.code = 4;
                    errorDetails.detail = CommonMessage.IncorrectUserRole;
                    errorResponse.errors.Add(errorDetails);
                    return (errorResponse, null);
                }

                string institutionIds = string.Empty;
                try
                {
                    var client = new RestClient(_appSettings.Host + _dependencies.InstitutionsUrl + ObfuscationClass.EncodeId(user.UserId, _appSettings.Prime).ToString());
                    var request = new RestRequest(Method.GET);
                    IRestResponse driverResponse = client.Execute(request);
                    if (driverResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var result = driverResponse.Content;
                        var institutionData = JsonConvert.DeserializeObject<InstitutionResponse>(result);
                        institutionIds = String.Join(",", institutionData.data.Select(x => x.InstitutionId));
                    }
                }
                catch (Exception)
                {
                    institutionIds = string.Empty;
                }

                TokenGenerator tokenGenerator = new TokenGenerator()
                {
                    UserId = ObfuscationClass.EncodeId(user.UserId, _appSettings.Prime).ToString(),
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = _context.Phones.Where(x => x.UserId == user.UserId).Select(x => x.Number).FirstOrDefault(),
                    Password = user.Password,
                    Roles = usersRoles,
                    InstitutionId = institutionIds
                };
                string Token = _helper.GenerateToken(tokenGenerator, Application);

                user.LastLoginDate = DateTime.Now;
                _context.Users.Update(user);
                _context.SaveChanges();
                response.message = CommonMessage.LoginSuccess;
                response.status = true;
                response.token = Token;
                response.statusCode = StatusCodes.Status200OK;
                return (null, response);
            }
            catch (Exception ex)
            {
                errorDetails.statusCode = StatusCodes.Status500InternalServerError;
                errorDetails.code = 3;
                errorDetails.detail = CommonMessage.ExceptionMessage + ex.Message;
                errorResponse.errors.Add(errorDetails);
                return (errorResponse, null);
            }
        }

        public async Task<dynamic> ChangePassword(ChangePasswordModel model)
        {
            UsersResponse response = new UsersResponse();
            string originalPassword = string.Empty;
            try
            {
                int UserId = ObfuscationClass.DecodeId(Convert.ToInt32(model.UserId), _appSettings.PrimeInverse);
                Users user = new Users();
                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                user = _context.Users.Where(x => x.UserId == UserId).FirstOrDefault();
                if (user == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.UserNotFound, StatusCodes.Status404NotFound);
                
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    originalPassword = await PasswordDecryptionAsync(model.NewPassword);
                    if (originalPassword == "Unauthorized Access")
                        return ReturnResponse.ErrorResponse(CommonMessage.IncorrectPassword, StatusCodes.Status400BadRequest);
                }

                user.Password = _passwordHasherRepository.Hash(originalPassword);
                _context.Users.Update(user);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.ChangePasswordSuccess, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> ForgotPassword(string email)
        {
            EmailResponse response = new EmailResponse();
            try
            {
                if (string.IsNullOrEmpty(email))
                    return ReturnResponse.ErrorResponse(CommonMessage.BadRequest, StatusCodes.Status400BadRequest);

                var users = _context.Users.Where(x => x.Email.ToLower() == email.ToLower()).FirstOrDefault();
                if (users == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.EmailNotFound, StatusCodes.Status404NotFound);

                var res = await _helper.VerifyEmail(email, users);
                if (res.StatusCode != HttpStatusCode.Accepted)
                    return ReturnResponse.ErrorResponse(CommonMessage.ForgotPasswordFailed, StatusCodes.Status500InternalServerError);

                return ReturnResponse.SuccessResponse(CommonMessage.ForgotPasswordSuccess, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<string> PasswordDecryptionAsync(string Password)
        {
            if (encryption.IndexOfBSign(Password) != -1)
                return await encryption.DecodeAndDecrypt(Password, _appSettings.IVForDashboard, _appSettings.KeyForDashboard);
            else
                return await encryption.DecodeAndDecrypt(Password, _appSettings.IVForAndroid, _appSettings.KeyForAndroid);
        }
    }
}
