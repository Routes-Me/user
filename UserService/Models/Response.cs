using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Models
{
    public class Response
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ResponseCode responseCode { get; set; }
    }
    public enum ResponseCode
    {
        Success = 200,
        Error = 2,
        InternalServerError = 500,
        MovedPermanently = 301,
        NotFound = 404,
        BadRequest = 400,
        Conflict = 409,
        Created = 201,
        NotAcceptable = 406,
        Unauthorized = 401,
        RequestTimeout = 408,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        Permissionserror = 403,
        Forbidden = 403,
        TokenRequired = 499,
        InvalidToken = 498
    }

    #region UserRoles Response
    public class RolesResponse : Response { }
    public class RolesGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<RolesModel> data { get; set; }
    }
    #endregion


    #region Login Response
    public class SignInV2Response : Response { }
    public class SignInResponse : Response
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string token { get; set; }
    }
    #endregion

    #region User Response
    public class UsersResponse : Response { }

    public class UsersGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<UsersModel> data { get; set; }
    }

    public class UsersAvatarResponse : Response { }
    #endregion

    public class EmailResponse : Response { }

    public class DriversModel
    {
        public int? UserId { get; set; }
        public int? InstitutionId { get; set; }
    }
}
