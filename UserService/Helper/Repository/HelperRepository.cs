using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using UserService.Helper.Abstraction;
using UserService.Models.Common;
using UserService.Models.ResponseModel;

namespace UserService.Helper.Repository
{
    public class HelperRepository : IHelperRepository
    {
        private readonly AppSettings _appSettings;
        private readonly SendGridSettings _sendGridSettings;

        public HelperRepository(IOptions<AppSettings> appSettings, IOptions<SendGridSettings> sendGridSettings)
        {
            _appSettings = appSettings.Value;
            _sendGridSettings = sendGridSettings.Value;
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
                new Claim(ClaimTypes.Email, Model.Email.ToString()),
                new Claim(ClaimTypes.Role, Model.RoleName.ToString()),
                new Claim(ClaimTypes.UserData, Model.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, Model.UserId.ToString()),
                };

                string token = string.Empty;
                if (Application.Count > 0 && Application.ToString().ToLower() == "screen")
                {
                    var tokenString = new JwtSecurityToken(
                                        issuer: _appSettings.ValidIssuer,
                                        audience: _appSettings.ValidAudience,
                                        expires: DateTime.UtcNow.AddYears(100),
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
        public async Task<SendGrid.Response> VerifyEmail(string email, string password)
        {
            try
            {
                var client = new SendGridClient(_sendGridSettings.APIKey);
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(_sendGridSettings.From, _sendGridSettings.Name),
                    Subject = _sendGridSettings.SubjectForgotPassword,
                    HtmlContent = "Password: <b>" + password + "</b>"
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
    }
}
