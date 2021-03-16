using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RoutesSecurity;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.DBModels;
using UserService.Models.Common;
using UserService.Models.ResponseModel;

namespace UserService.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _usersRepository;
        private readonly UsersServiceContext _context;
        private readonly AppSettings _appSettings;
        public UserController(IUserRepository usersRepository, UsersServiceContext context, IOptions<AppSettings> appSettings)
        {
            _usersRepository = usersRepository;
            _context = context;
            _appSettings = appSettings.Value;
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

        [HttpPost]
        [Route("users")]
        public async Task<IActionResult> PostUser(UsersDto usersDto)
        {
            PostUserResponse response = new PostUserResponse();
            try
            {
                Users user = _usersRepository.PostUser(usersDto);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                response.UserId = Obfuscation.Encode(user.UserId);
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            response.Message = CommonMessage.UserInsert;
            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}