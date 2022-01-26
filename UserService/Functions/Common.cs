using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using UAParser;
using UserService.Models.Common;

namespace UserService.Functions
{
    public class Common
    {
        public static JArray SerializeJsonForIncludedRepo(List<dynamic> objList)
        {
            var modelsJson = JsonConvert.SerializeObject(objList,
                                 new JsonSerializerSettings
                                 {
                                     NullValueHandling = NullValueHandling.Ignore,
                                 });

            return JArray.Parse(modelsJson);
        }

        public static VerificationHeader ParseJwtHeader(string Jwttoken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(Jwttoken);

            string sub = token.Claims.First(claim => claim.Type == "sub").Value;
            var exp = Convert.ToInt64(token.Claims.First(claim => claim.Type == "exp").Value);

            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(exp);

            VerificationHeader header = new VerificationHeader();
            header.Subject = sub;
            header.ExpiryTime = dateTime;

            return header;
        }

        public static ClientInfo GetOs(string UserAgent)
        {
            var uaParser = Parser.GetDefault();
            ClientInfo _ci = uaParser.Parse(UserAgent);

            return _ci;
        }
    }
}
