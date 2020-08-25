using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserRepository _usersRepository;
        private static readonly HttpClient HttpClient = new HttpClient();
        public UserController(IUserRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        [HttpGet]
        [Route("users/{id=0}")]
        public IActionResult Get(int id, [FromQuery] Pagination pageInfo)
        {
            UsersGetResponse response = new UsersGetResponse();
            response = _usersRepository.GetUser(id, pageInfo);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpDelete]
        [Route("users/{id}")]
        public IActionResult delete(int id)
        {
            UsersResponse response = new UsersResponse();
            if (ModelState.IsValid)
                response = _usersRepository.DeleteUser(id);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPut]
        [Route("users")]
        public IActionResult Put(RegistrationModel model)
        {
            UsersResponse response = new UsersResponse();
            if (ModelState.IsValid)
                response = _usersRepository.UpdateUser(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}
