using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _usersRepository;
        private static readonly HttpClient HttpClient = new HttpClient();
        public UserController(IUserRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        [HttpGet]
        [Route("users/{id=0}")]
        public IActionResult Get(string id, string Include, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _usersRepository.GetUser(id, pageInfo, Include);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("users/{id}")]
        public IActionResult delete(string id)
        {
            dynamic response = _usersRepository.DeleteUser(id);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("users")]
        public IActionResult Put(RegistrationModel model)
        {
            dynamic response = _usersRepository.UpdateUser(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpGet]
        [Route("institutions/{institutionsId}/users/{id=0}")]
        public IActionResult GetFilteredUsers(string institutionsId, string id, string Include, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _usersRepository.GetFilteredUsers(institutionsId, id, pageInfo, Include);
            return StatusCode((int)response.statusCode, response);
        }
    }
}