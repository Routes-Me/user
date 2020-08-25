using Microsoft.Extensions.Options;
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
        public string GenerateToken(TokenGenerator Model)
        {
            if (Model == null)
                return null;

            //var handler = new JwtSecurityTokenHandler();

            //var identity = new ClaimsIdentity(new GenericIdentity(Model.Email), new[] { new Claim("Email", Model.Email.ToString()) });
            //identity.AddClaim(new Claim("UserId", Model.UserId.ToString()));
            //identity.AddClaim(new Claim("Role", Model.RoleName.ToString()));
            //var key = Encoding.UTF8.GetBytes(_appSettings.Secret);
            //var tokenString = handler.CreateJwtSecurityToken(
            //subject: identity,
            //issuer: _appSettings.ValidIssuer,
            //audience: _appSettings.ValidAudience,
            //expires: DateTime.UtcNow.AddDays(7),
            //signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //);
            //var token =  handler.WriteToken(tokenString);


            var key = Encoding.UTF8.GetBytes(_appSettings.Secret);
            var claimsData = new Claim[]
            {
                new Claim(ClaimTypes.Email, Model.Email.ToString()),
                new Claim(ClaimTypes.Role, Model.RoleName.ToString()),
                new Claim(ClaimTypes.UserData, Model.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, Model.UserId.ToString()),
            };
            var tokenString = new JwtSecurityToken(
            issuer: _appSettings.ValidIssuer,
            audience: _appSettings.ValidAudience,
            expires: DateTime.UtcNow.AddDays(7),
            claims: claimsData,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );
            var token = new JwtSecurityTokenHandler().WriteToken(tokenString);
            return token;
        }

        public async Task<SendGrid.Response> SendConfirmationEmail(int userId, string email, string siteUrl)
        {
            var client = new SendGridClient(_sendGridSettings.APIKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridSettings.From, _sendGridSettings.Name),
                Subject = _sendGridSettings.SubjectEmailVerify,
                HtmlContent = "Please <a href='" + siteUrl + "/api/email/confirm?id=" + userId + "'>click here</a> to verify."
            };
            msg.AddTo(new EmailAddress(email));
            msg.SetClickTracking(false, false);
            return await client.SendEmailAsync(msg);
        }
        public async Task<SendGrid.Response> VerifyEmail(string email, string password)
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
    }
}
