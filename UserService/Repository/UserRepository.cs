using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UserService.Abstraction;
using UserService.Helper.Abstraction;
using UserService.Helper.Common;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;
using System.Runtime;
using RestSharp.Serialization;

namespace UserService.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly userserviceContext _context;
        private readonly IHelperRepository _helper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVerificationRepository _verificationRepository;
        private readonly IPasswordHasherRepository _passwordHasherRepository;
        private readonly AppSettings _appSettings;
        private readonly AzureStorageBlobConfig _config;
        public UserRepository(IOptions<AzureStorageBlobConfig> config, IOptions<AppSettings> appSettings, userserviceContext context, IHelperRepository helper, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IVerificationRepository verificationRepository, IPasswordHasherRepository passwordHasherRepository)
        {
            _config = config.Value;
            _appSettings = appSettings.Value;
            _verificationRepository = verificationRepository;
            _context = context;
            _helper = helper;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasherRepository = passwordHasherRepository;
        }

        public SignInResponse SignIn(SigninModel model)
        {
            SignInResponse response = new SignInResponse();
            try
            {
                Users user = new Users();
                if (string.IsNullOrEmpty(model.Username))
                {
                    response.status = false;
                    response.message = "Invalid username.";
                    response.token = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }
                
                if (string.IsNullOrEmpty(model.Password))
                {
                    response.status = false;
                    response.message = "Invalid password.";
                    response.token = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }


                user = _context.Users.Where(x => x.Email == model.Username).FirstOrDefault();
                if (user == null)
                {
                    var phoneUser = _context.Phones.Where(x => x.Number == model.Username).FirstOrDefault();
                    if (phoneUser == null)
                    {
                        response.status = false;
                        response.message = "User not found.";
                        response.token = null;
                        response.responseCode = ResponseCode.NotFound;
                        return response;
                    }

                    user = _context.Users.Where(x => x.UserId == phoneUser.UserId).FirstOrDefault();
                    if (user == null)
                    {
                        response.status = false;
                        response.message = "User not found.";
                        response.token = null;
                        response.responseCode = ResponseCode.NotFound;
                        return response;
                    }

                    if (phoneUser.Number != model.Username && !_passwordHasherRepository.Check(user.Password, AesBase64Wrapper.DecodeAndDecrypt(model.Password)).Verified)
                    {
                        response.status = false;
                        response.message = "Login Failed: Incorrect phone or password!";
                        response.token = null;
                        response.responseCode = ResponseCode.Unauthorized;
                        return response;
                    }
                }
                else
                {
                    if (user.Email != model.Username && !_passwordHasherRepository.Check(user.Password, AesBase64Wrapper.DecodeAndDecrypt(model.Password)).Verified)
                    {
                        response.status = false;
                        response.message = "Login Failed: Incorrect phone or password!";
                        response.token = null;
                        response.responseCode = ResponseCode.Unauthorized;
                        return response;
                    }
                }
               
                var usersRole = (from usersrole in _context.UsersRoles
                                 join role in _context.Roles on usersrole.RoleId equals role.RoleId
                                 where usersrole.UserId == user.UserId
                                 select new Roles
                                 {
                                     RoleId = role.RoleId,
                                     Application = role.Application,
                                     Description = role.Description,
                                     Name = role.Name
                                 }).FirstOrDefault();

                if (usersRole == null)
                {
                    response.status = false;
                    response.message = "Role not found.";
                    response.token = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                TokenGenerator tokenGenerator = new TokenGenerator()
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    RoleName = usersRole.Name
                };
                string Token = _helper.GenerateToken(tokenGenerator);

                _context.Users.Update(user);
                _context.SaveChanges();

                response.message = "Login successfully.";
                response.status = true;
                response.token = Token;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while login. Error Message - " + ex.Message;
                response.token = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public UsersResponse SignUp(RegistrationModel model)
        {
            UsersResponse response = new UsersResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                if (model.Roles.Count == 0)
                {
                    response.status = false;
                    response.message = "User role not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                foreach (var role in model.Roles)
                {
                    var userRolesData = _context.Roles.Where(x => x.RoleId == role).FirstOrDefault();
                    if (userRolesData == null)
                    {
                        response.status = false;
                        response.message = "User role not found.";
                        response.responseCode = ResponseCode.NotFound;
                        return response;
                    }
                }

                var phoneData = _context.Phones.Where(x => x.Number == model.PhoneNumber).FirstOrDefault();
                if (phoneData != null)
                {
                    response.status = false;
                    response.message = "Phone number already exist.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var userEmailData = _context.Users.Where(x => x.Email == model.Email).FirstOrDefault();
                if (userEmailData != null)
                {
                    response.status = false;
                    response.message = "Email already exist.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                Users users = new Users()
                {
                    Name = model.Name,
                    Email = model.Email,
                    CreatedAt = DateTime.UtcNow,
                    Password = _passwordHasherRepository.Hash(AesBase64Wrapper.DecodeAndDecrypt(model.Password)),
                    IsEmailVerified = false
                };
                _context.Users.Add(users);
                _context.SaveChanges();

                Phones phone = new Phones()
                {
                    UserId = users.UserId,
                    IsVerified = false,
                    Number = model.PhoneNumber,
                };
                _context.Phones.Add(phone);
                _context.SaveChanges();

                foreach (var role in model.Roles)
                {
                    UsersRoles usersroles = new UsersRoles()
                    {
                        UserId = users.UserId,
                        RoleId = role
                    };
                    _context.UsersRoles.Add(usersroles);
                }
                _context.SaveChanges();

                DriversModel driver = new DriversModel()
                {
                    InstitutionId = model.InstitutionId,
                    UserId = 1
                };

                var client = new RestClient(_appSettings.VehicleEndpointUrl);
                var request = new RestRequest(Method.POST);
                string jsonToSend = JsonConvert.SerializeObject(driver);
                request.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;
                IRestResponse institutionResponse = client.Execute(request);
                if (institutionResponse.StatusCode != HttpStatusCode.Created)
                {
                    response.status = false;
                    response.message = "Something went wrong while inserting user. Error Message - Unable to insert user into drivers." + institutionResponse.Content;
                    response.responseCode = ResponseCode.InternalServerError;
                    return response;
                }

                response.status = true;
                response.message = "User created successfully.";
                response.responseCode = ResponseCode.Created;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting user. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public UsersResponse DeleteUser(int id)
        {
            UsersResponse response = new UsersResponse();
            try
            {
                var usersData = _context.Users.Where(x => x.UserId == id).FirstOrDefault();
                if (usersData == null)
                {
                    response.status = false;
                    response.message = "User not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var usersRoles = _context.UsersRoles.Where(x => x.UserId == id);
                if (usersRoles != null)
                {
                    _context.UsersRoles.RemoveRange(usersRoles);
                    _context.SaveChanges();
                }

                var phone = _context.Phones.Where(x => x.UserId == id);
                if (phone != null)
                {
                    _context.Phones.RemoveRange(phone);
                    _context.SaveChanges();
                }

                _context.Users.Remove(usersData);
                _context.SaveChanges();
                response.status = true;
                response.message = "User deleted successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting user. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public UsersResponse UpdateUser(RegistrationModel model)
        {
            UsersResponse response = new UsersResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var userData = _context.Users.Where(x => x.UserId == model.UserId).FirstOrDefault();
                if (userData == null)
                {
                    response.status = false;
                    response.message = "User not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                foreach (var role in model.Roles)
                {
                    var userRolesData = _context.Roles.Where(x => x.RoleId == role).FirstOrDefault();
                    if (userRolesData == null)
                    {
                        response.status = false;
                        response.message = "User role not found.";
                        response.responseCode = ResponseCode.NotFound;
                        return response;
                    }
                }

                var usersRolesData = _context.UsersRoles.Where(x => x.UserId == model.UserId);
                if (usersRolesData != null)
                {
                    _context.UsersRoles.RemoveRange(usersRolesData);
                    _context.SaveChanges();
                }

                foreach (var role in model.Roles)
                {
                    UsersRoles usersroles = new UsersRoles()
                    {
                        UserId = model.UserId,
                        RoleId = role
                    };
                    _context.UsersRoles.Add(usersroles);
                }
                _context.SaveChanges();


                var userPhone = _context.Phones.Where(x => x.UserId == model.UserId).FirstOrDefault();
                if (userPhone == null)
                {
                    Phones newPhone = new Phones()
                    {
                        IsVerified = false,
                        Number = model.PhoneNumber,
                        UserId = model.UserId
                    };
                    _context.Phones.Add(newPhone);
                }
                else if (userPhone.Number != model.PhoneNumber)
                {
                    userPhone.Number = model.PhoneNumber;
                    userPhone.IsVerified = false;
                    _context.Phones.Update(userPhone);
                    _context.SaveChanges();
                }

                if (userData.Email != model.Email)
                    userData.IsEmailVerified = false;

                userData.Name = model.Name;
                userData.Email = model.Email;
                _context.Users.Update(userData);
                _context.SaveChanges();
                response.status = true;
                response.message = "User updated successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating user. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public UsersGetResponse GetUser(int userId, Pagination pageInfo)
        {
            UsersGetResponse response = new UsersGetResponse();
            int totalCount = 0;
            try
            {
                List<UsersModel> usersModelList = new List<UsersModel>();

                if (userId == 0)
                {
                    usersModelList = (from user in _context.Users
                                      join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                      join role in _context.Roles on usersRole.RoleId equals role.RoleId
                                      join phone in _context.Phones on user.UserId equals phone.UserId
                                      select new UsersModel
                                      {
                                          UserId = user.UserId,
                                          Phone = phone.Number,
                                          Email = user.Email,
                                          CreatedAt = user.CreatedAt,
                                          Name = user.Name,
                                          Application = role.Application,
                                          Description = role.Description
                                      }).ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from user in _context.Users
                                  join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                  join role in _context.Roles on usersRole.RoleId equals role.RoleId
                                  join phone in _context.Phones on user.UserId equals phone.UserId
                                  select new UsersModel
                                  {
                                      UserId = user.UserId,
                                      Phone = phone.Number,
                                      Email = user.Email,
                                      CreatedAt = user.CreatedAt,
                                      Name = user.Name,
                                      Application = role.Application,
                                      Description = role.Description
                                  }).ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().Count();
                }
                else
                {
                    usersModelList = (from user in _context.Users
                                      join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                      join role in _context.Roles on usersRole.RoleId equals role.RoleId
                                      join phone in _context.Phones on user.UserId equals phone.UserId
                                      where user.UserId == userId
                                      select new UsersModel
                                      {
                                          UserId = user.UserId,
                                          Phone = phone.Number,
                                          Email = user.Email,
                                          CreatedAt = user.CreatedAt,
                                          Name = user.Name,
                                          Application = role.Application,
                                          Description = role.Description
                                      }).ToList().GroupBy(p => p.UserId).Select(g => g.First()).OrderBy(a => a.UserId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from user in _context.Users
                                  join usersRole in _context.UsersRoles on user.UserId equals usersRole.UserId
                                  join role in _context.Roles on usersRole.RoleId equals role.RoleId
                                  join phone in _context.Phones on user.UserId equals phone.UserId
                                  where user.UserId == userId
                                  select new UsersModel
                                  {
                                      UserId = user.UserId,
                                      Phone = phone.Number,
                                      Email = user.Email,
                                      CreatedAt = user.CreatedAt,
                                      Name = user.Name,
                                      Application = role.Application,
                                      Description = role.Description
                                  }).ToList().GroupBy(p => p.UserId).Select(g => g.First()).ToList().Count();
                }

                if (usersModelList == null || usersModelList.Count == 0)
                {
                    response.status = false;
                    response.message = "User not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.message = "Users data retrived successfully.";
                response.status = true;
                response.pagination = page;
                response.data = usersModelList;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while fetching user. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public UsersResponse ChangePassword(ChangePasswordModel model)
        {
            UsersResponse response = new UsersResponse();
            try
            {
                Users user = new Users();
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                user = _context.Users.Where(x => x.Email == model.Username).FirstOrDefault();
                if (user == null)
                {
                    var phoneUser = _context.Phones.Where(x => x.Number == model.Username).FirstOrDefault();
                    if (phoneUser == null)
                    {
                        response.status = false;
                        response.message = "User not found.";
                        response.responseCode = ResponseCode.NotFound;
                        return response;
                    }

                    user = _context.Users.Where(x => x.UserId == phoneUser.UserId).FirstOrDefault();
                    if (user == null)
                    {
                        response.status = false;
                        response.message = "User not found.";
                        response.responseCode = ResponseCode.NotFound;
                        return response;
                    }

                    if (phoneUser.Number != model.Username && !_passwordHasherRepository.Check(user.Password, AesBase64Wrapper.DecodeAndDecrypt(model.CurrentPassword)).Verified)
                    {
                        response.status = false;
                        response.message = "Login Failed: Incorrect phone or password!";
                        response.responseCode = ResponseCode.Unauthorized;
                        return response;
                    }
                }
                else
                {
                    if (user.Email != model.Username && !_passwordHasherRepository.Check(user.Password, AesBase64Wrapper.DecodeAndDecrypt(model.CurrentPassword)).Verified)
                    {
                        response.status = false;
                        response.message = "Login Failed: Incorrect phone or password!";
                        response.responseCode = ResponseCode.Unauthorized;
                        return response;
                    }
                }

                user.Password = _passwordHasherRepository.Hash(AesBase64Wrapper.DecodeAndDecrypt(model.NewPassword));
                _context.Users.Update(user);
                _context.SaveChanges();
                response.status = true;
                response.message = "Password updated successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating password. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public async Task<EmailResponse> SendConfirmationEmail(EmailModel model)
        {
            EmailResponse response = new EmailResponse();
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    response.status = false;
                    response.message = "Pass valid email address.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var userData = _context.Users.Where(x => x.UserId == model.UserId).FirstOrDefault();
                if (userData == null)
                {
                    response.status = false;
                    response.message = "User not found.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                if (userData.Email.ToLower() != model.Email.ToLower())
                {
                    response.status = false;
                    response.message = "Email does not belong to this user.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                string host = _httpContextAccessor.HttpContext.Request.Host.ToString();
                string protocol = _httpContextAccessor.HttpContext.Request.Scheme;
                string siteUrl = protocol + "://" + host;

                var res = await _helper.SendConfirmationEmail(model.UserId, model.Email, siteUrl);

                if (res.StatusCode != HttpStatusCode.Accepted)
                {
                    response.status = false;
                    response.message = "Something went wrong while sending verification email. Error Message -" + res.Body.ReadAsStringAsync();
                    response.responseCode = ResponseCode.InternalServerError;
                    return response;
                }

                response.status = true;
                response.message = "Email verification sent. Please Check your inbox.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while sending verification email. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public EmailResponse VerifyEmail(int userId)
        {
            EmailResponse response = new EmailResponse();
            try
            {
                var usersData = _context.Users.Where(x => x.UserId == userId).FirstOrDefault();
                if (usersData == null)
                {
                    response.status = false;
                    response.message = "User not found.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                usersData.IsEmailVerified = true;
                _context.Users.Update(usersData);
                _context.SaveChanges();

                response.status = true;
                response.message = "Email verified successfully";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while verifying email. Error Message -" + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public async Task<EmailResponse> ForgotPassword(string email)
        {
            EmailResponse response = new EmailResponse();
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    response.status = false;
                    response.message = "Pass valid email address.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var users = _context.Users.Where(x => x.Email.ToLower() == email.ToLower()).FirstOrDefault();
                if (users == null)
                {
                    response.status = false;
                    response.message = "Email does not exist in the system.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var res = await _helper.VerifyEmail(email, users.Password);
                if (res.StatusCode != HttpStatusCode.Accepted)
                {
                    response.status = false;
                    response.message = "Something went wrong while sending password to your email. Error Message - " + res.Body.ReadAsStringAsync();
                    response.responseCode = ResponseCode.InternalServerError;
                    return response;
                }

                response.status = true;
                response.message = "Password sent to your email. Please Check your inbox.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while sending password to your email. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public SignInV2Response CheckPhoneNumberExist(string phoneNumber)
        {
            SignInV2Response response = new SignInV2Response();
            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    response.status = false;
                    response.message = "Pass valid phone number.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }
                var phone = _context.Phones.Where(x => x.Number == phoneNumber).FirstOrDefault();
                if (phone == null)
                {
                    response.status = false;
                    response.message = "Phone number does not exist in system.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                response.status = true;
                response.message = "Phone already exist in system.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while varifying your phone number. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.Success;
                return response;
            }
        }

        public SignInV2Response ConfirmPhoneNumber(string phoneNumber)
        {
            SignInV2Response response = new SignInV2Response();
            try
            {
                var phone = _context.Phones.Where(x => x.Number == phoneNumber).FirstOrDefault();
                if (phone == null)
                {
                    response.status = false;
                    response.message = "Phone number does not exist in system.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                phone.IsVerified = true;
                _context.Phones.Update(phone);
                _context.SaveChanges();
                response.status = true;
                response.message = "Phone verified successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while confirming your phone. Error Message - " + new VerificationResult(new List<string> { ex.Message });
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public async Task<SignInV2Response> SendSignInOTP(SignInOTPModel model)
        {
            SignInV2Response response = new SignInV2Response();
            try
            {
                if (string.IsNullOrEmpty(model.Phone))
                {
                    response.status = false;
                    response.message = "Please pass valid phone number.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var phone = _context.Phones.Where(x => x.Number == model.Phone).FirstOrDefault();
                if (phone == null)
                {
                    response.status = false;
                    response.message = "Phone number does not exist in the system.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                return await _verificationRepository.SendOTP(model.Phone);
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while login. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public async Task<SignInResponse> VerifySignInOTP(VerifySignInOTPModel model)
        {
            SignInResponse response = new Models.SignInResponse();
            try
            {
                if (string.IsNullOrEmpty(model.Phone))
                {
                    response.status = false;
                    response.message = "Please pass valid phone number.";
                    response.token = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                if (string.IsNullOrEmpty(model.Code))
                {
                    response.status = false;
                    response.message = "Please pass valid OTP code.";
                    response.token = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var phone = _context.Phones.Where(x => x.Number == model.Phone).FirstOrDefault();
                if (phone == null)
                {
                    response.status = false;
                    response.message = "The phoneNumber does not exits in system.";
                    response.token = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var user = _context.Users.Where(x => x.UserId == phone.UserId).FirstOrDefault();
                if (phone == null)
                {
                    response.status = false;
                    response.message = "The user does not exits in system.";
                    response.token = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var associatedRole = _context.UsersRoles.Where(x => x.UserId == phone.UserId).FirstOrDefault();
                if (associatedRole == null)
                {
                    response.status = false;
                    response.message = "The user is not associated with any role.";
                    response.token = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var role = _context.Roles.Where(x => x.RoleId == associatedRole.RoleId).FirstOrDefault();
                if (role == null)
                {
                    response.status = false;
                    response.message = "User not found.";
                    response.token = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var result = await _verificationRepository.VerifyOTP(model.Phone, model.Code);
                if (result.responseCode != ResponseCode.Success)
                {
                    response.status = false;
                    response.message = result.message;
                    response.token = null;
                    response.responseCode = result.responseCode;
                    return response;
                }

                
                TokenGenerator tokenGenerator = new TokenGenerator()
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    RoleName = role.Name
                };
                string Token = _helper.GenerateToken(tokenGenerator);

                response.message = "Login successfully.";
                response.status = true;
                response.token = Token;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while login. Error Message - " + ex.Message;
                response.token = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}