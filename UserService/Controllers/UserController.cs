using System.Threading.Tasks;
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
        public UserController(IUserRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        [HttpGet]
        [Route("users/{id?}")]
        public IActionResult Get(string id, string Include, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _usersRepository.GetUser(id, pageInfo, Include);
            return StatusCode(response.statusCode, response);
        }

        [HttpDelete]
        [Route("users/{id}")]
        public IActionResult delete(string id)
        {
            dynamic response = _usersRepository.DeleteUser(id);
            return StatusCode(response.statusCode, response);
        }

        [HttpPut]
        [Route("users")]
        public IActionResult Put(UsersDto usersDto)
        {
            dynamic response = _usersRepository.UpdateUser(usersDto);
            return StatusCode(response.statusCode, response);
        }
    }
}