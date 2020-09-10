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
        public IActionResult Get(int id, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _usersRepository.GetUser(id, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("users/{id}")]
        public IActionResult delete(int id)
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
    }
}