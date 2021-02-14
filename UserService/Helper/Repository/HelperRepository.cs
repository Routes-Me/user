using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Obfuscation;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UserService.Helper.Abstraction;
using UserService.Models.Common;
using UserService.Models;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Helper.Repository
{
    public class HelperRepository : IHelperRepository
    {
        private readonly AppSettings _appSettings;
        private readonly SendGridSettings _sendGridSettings;
        private readonly userserviceContext _context;

        public HelperRepository(IOptions<AppSettings> appSettings, IOptions<SendGridSettings> sendGridSettings, userserviceContext context)
        {
            _appSettings = appSettings.Value;
            _sendGridSettings = sendGridSettings.Value;
            _context = context;
        }
        public string GenerateToken(TokenGenerator Model, StringValues Application)
        {
            try
            {
                if (Model == null)
                    return null;

                var key = Encoding.UTF8.GetBytes(_appSettings.Secret);
                var claimsData = new Claim[]
                {
                    new Claim("UserId", string.IsNullOrEmpty(Model.UserId) ? string.Empty : Model.UserId.ToString()),
                    new Claim("Name", string.IsNullOrEmpty(Model.Name) ? string.Empty : Model.Name.ToString()),
                    new Claim("Email", string.IsNullOrEmpty(Model.Email) ? string.Empty : Model.Email.ToString()),
                    new Claim("PhoneNumber", string.IsNullOrEmpty(Model.PhoneNumber) ? string.Empty :Model.PhoneNumber.ToString()),
                    new Claim("Password", string.IsNullOrEmpty(Model.Password)? string.Empty : Model.Password.ToString()),
                    new Claim("Roles", JsonConvert.SerializeObject(Model.Roles)),
                    new Claim("InstitutionId", string.IsNullOrEmpty(Model.InstitutionId) ? string.Empty :Model.InstitutionId.ToString())
                };
                string token = string.Empty;
                if (Application.Count > 0 && Application.ToString().ToLower() == "screen")
                {
                    var tokenString = new JwtSecurityToken(
                                        issuer: _appSettings.ValidIssuer,
                                        audience: _appSettings.ValidAudience,
                                        expires: DateTime.UtcNow.AddMonths(6),
                                        claims: claimsData,
                                        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                                        );
                    token = new JwtSecurityTokenHandler().WriteToken(tokenString);
                }
                else
                {
                    var tokenString = new JwtSecurityToken(
                                       issuer: _appSettings.ValidIssuer,
                                       audience: _appSettings.ValidAudience,
                                       expires: DateTime.UtcNow.AddMinutes(60),
                                       claims: claimsData,
                                       signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                                       );
                    token = new JwtSecurityTokenHandler().WriteToken(tokenString);
                }
                return token;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GenerateAccessToken(AccessTokenGenerator accessTokenGenerator, StringValues application)
        {
            try
            {
                if (accessTokenGenerator == null)
                    throw new ArgumentNullException(CommonMessage.TokenDataNull);

                var key = Encoding.UTF8.GetBytes(_appSettings.AccessSecretKey);
                var claimsData = new Claim[]
                {
                    new Claim("nme", string.IsNullOrEmpty(accessTokenGenerator.Name) ? string.Empty : accessTokenGenerator.Name.ToString()),
                    new Claim("sub", string.IsNullOrEmpty(accessTokenGenerator.UserId) ? string.Empty : accessTokenGenerator.UserId.ToString()),
                    new Claim("rol", JsonConvert.SerializeObject(accessTokenGenerator.Roles)),
                    application.ToString().ToLower() == "dashboard"
                        ? new Claim("ins", string.IsNullOrEmpty(accessTokenGenerator.InstitutionId) ? string.Empty : accessTokenGenerator.InstitutionId.ToString())
                        : null,
                };

                var tokenString = new JwtSecurityToken(
                                    issuer: _appSettings.SessionTokenIssuer,
                                    audience: GetAudience(application),
                                    expires: application.ToString().ToLower() == "screen" ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddMinutes(15),
                                    claims: claimsData,
                                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                                    );
                return new JwtSecurityTokenHandler().WriteToken(tokenString);
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GenerateRefreshToken(StringValues application, string accessToken)
        {
            var key = Encoding.UTF8.GetBytes(_appSettings.RefreshSecretKey);
            var claimsData = new Claim[]
            {
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("stamp", ScrambleSignature(accessToken.Split('.').Last()))
            };

            var tokenString = new JwtSecurityToken(
                                issuer: _appSettings.SessionTokenIssuer,
                                audience: GetAudience(application),
                                expires: application.ToString().ToLower() == "screen" ? DateTime.UtcNow.AddMonths(6) : DateTime.UtcNow.AddMonths(3),
                                claims: claimsData,
                                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                                );
            return new JwtSecurityTokenHandler().WriteToken(tokenString);
        }

        public async Task<SendGrid.Response> SendConfirmationEmail(int userId, string email, string siteUrl)
        {
            try
            {
                var client = new SendGridClient(_sendGridSettings.APIKey);
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(_sendGridSettings.From, _sendGridSettings.Name),
                    Subject = _sendGridSettings.SubjectEmailVerify,
                    HtmlContent = "Please <a href='" + siteUrl + "?id=" + userId + "'>click here</a> to verify."
                };
                msg.AddTo(new EmailAddress(email));
                msg.SetClickTracking(false, false);
                return await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<SendGrid.Response> VerifyEmail(string email, Users users)
        {
            try
            {
                bool IsRoutesApp = false;
                foreach (var item in users.UsersRoles)
                {
                    var Application = _context.Applications.Where(x => x.ApplicationId == item.ApplicationId).FirstOrDefault();
                    if (Application != null)
                    {
                        if (Application.ToString().ToLower() == "userapp")
                        {
                            IsRoutesApp = true;
                        }
                    }
                }
                string UserId = ObfuscationClass.EncodeId(users.UserId, _appSettings.Prime).ToString();
                var client = new SendGridClient(_sendGridSettings.APIKey);
                if (IsRoutesApp == true)
                {
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress(_sendGridSettings.From, _sendGridSettings.Name),
                        Subject = _sendGridSettings.SubjectForgotPassword,
                        HtmlContent = "<div style='background-color: white;max-width: 414px;margin: 0 auto;padding: 20px;min-width: 600px; line-height: 1.5; font-size: 18px;'> <h3>Hi " + users.Name + ",</h3><div>You recently requested to reset your password for your RoutesApp account. Click the button below to reset it.</div><div style='border-radius: 10px;background-color: #1a82e2;text-align: center;width: max-content;margin: auto;margin-top: 30px;margin-bottom: 30px;'><a href=" + _appSettings.RoutesAppUrl + UserId + " target='_blank' style='display: inline-block; padding: 16px 36px; font-family: &#39Source Sans Pro&#39, Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;'>Reset your password</a></div><div> If you did not request a password reset, please ignore this email or reply to let us know. This password reset is only valid for the next 30 minutes.</div><br/><div>Thanks,</div><div>RoutesApp Team</div><br/><div>If you asre having trouble clicking the password reset button, copy and paste the URL below into your web browser.</div><div><a href=" + _appSettings.RoutesAppUrl + UserId + "  target='_blank'>" + _appSettings.RoutesAppUrl + UserId + " </a></div></div>"
                    };
                    msg.AddTo(new EmailAddress(email));
                    msg.SetClickTracking(false, false);
                    return await client.SendEmailAsync(msg);
                }
                else
                {
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress(_sendGridSettings.From, _sendGridSettings.Name),
                        Subject = _sendGridSettings.SubjectForgotPassword,
                        HtmlContent = "<div style='background-color: white;max-width: 414px;margin: 0 auto;padding: 20px;min-width: 600px; line-height: 1.5; font-size: 18px;'> <h3>Hi " + users.Name + ",</h3><div>You recently requested to reset your password for your RoutesApp account. Click the button below to reset it.</div><div style='border-radius: 10px;background-color: #1a82e2;text-align: center;width: max-content;margin: auto;margin-top: 30px;margin-bottom: 30px;'><a href=" + _appSettings.RoutesAppUrl + UserId + "  target='_blank' style='display: inline-block; padding: 16px 36px; font-family: &#39Source Sans Pro&#39, Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;'>Reset your password</a></div><div> If you did not request a password reset, please ignore this email or reply to let us know. This password reset is only valid for the next 30 minutes.</div><br/><div>Thanks,</div><div>RoutesApp Team</div><br/><div>If you asre having trouble clicking the password reset button, copy and paste the URL below into your web browser.</div><div><a href=" + _appSettings.RoutesAppUrl + UserId + "  target='_blank'>" + _appSettings.RoutesAppUrl + UserId + " </a></div></div>"
                    };
                    msg.AddTo(new EmailAddress(email));
                    msg.SetClickTracking(false, false);
                    return await client.SendEmailAsync(msg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes(plainText));
        }

        private static string GenerateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string ScrambleSignature(string signature)
        {
            string enc =  Base64Encode(signature);
            int index = enc[enc.Length - 3] % 7;
            if (index == 0)
                index = 7;
            return enc.Insert(index, GenerateRandomString(index));
        }

        private string GetAudience(StringValues application)
        {
            switch (application.ToString().ToLower())
            {
                case "dashboard": return "https://dashboard.routesme.com";
                case "app": return "https://app.routesme.com";
                case "screen": return "https://screen.routesme.com";
                default: return CommonMessage.UnknownApplication;
            }
        }
    }
}
