﻿using Microsoft.AspNetCore.Http;
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
        public int statusCode { get; set; }
    }

    public class ReturnResponse
    {
        public static dynamic ExceptionResponse(Exception ex)
        {
            Response response = new Response();
            response.status = false;
            response.message = CommonMessage.GenericException;
            response.statusCode = StatusCodes.Status401Unauthorized;
            return response;
        }

        public static dynamic SuccessResponse(string message, bool isCreated)
        {
            Response response = new Response();
            response.status = true;
            response.message = message;
            if (isCreated)
                response.statusCode = StatusCodes.Status201Created;
            else
                response.statusCode = StatusCodes.Status200OK;
            return response;
        }

        public static dynamic ErrorResponse(string message, int statusCode)
        {
            Response response = new Response();
            response.status = false;
            response.message = message;
            response.statusCode = statusCode;
            return response;
        }
    }

    public class ErrorDetails
    {
        public int statusCode { get; set; }
        public string detail { get; set; }
        public int code { get; set; }
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

    public class ErrorResponse
    {
        public List<ErrorDetails> errors { get; set; }
    }

    public class SignInResponse : Response
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string token { get; set; }
    }

    public class QrSignInResponse : Response
    {
        public LoginUser user { get; set; }
    }
    #endregion

    #region User Response
    public class UsersResponse : Response
    {
        public string UserId { get; set; }
        public string Email { get; set; }
    }

    public class UsersGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<UsersModel> data { get; set; }
    }

    #endregion

    public class EmailResponse : Response { }

    public class DriversModel
    {
        public string UserId { get; set; }
        public string InstitutionId { get; set; }
    }
}
