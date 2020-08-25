using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;

namespace UserService.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IActionResult GetActionResult(Response response)
        {
            switch (response.responseCode)
            {
                case ResponseCode.Success:
                    return Ok(response);
                case ResponseCode.Error:
                    return BadRequest(response);
                case ResponseCode.NotFound:
                    return NotFound(response);
                case ResponseCode.BadRequest:
                    return BadRequest(response);
                case ResponseCode.Conflict:
                    return Conflict(response);
                case ResponseCode.Unauthorized:
                    return Unauthorized(response);
                case ResponseCode.InternalServerError:
                    return BadRequest(response);
                case ResponseCode.Created:
                    return Created("Created", response);
                default:
                    return StatusCode((int)response.responseCode, response.message);
            }
        }
    }
}