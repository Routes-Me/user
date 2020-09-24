using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Abstraction;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Controllers
{
    [Route("api")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRolesRepository _rolesRepository;
        public RolesController(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }

        [HttpGet]
        [Route("roles/{id=0}")]
        public IActionResult Get(string id, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _rolesRepository.GetRoles(id, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("roles")]
        public IActionResult Post(RolesModel model)
        {
            dynamic response = _rolesRepository.InsertRoles(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("roles")]
        public IActionResult Put(RolesModel model)
        {
            dynamic response = _rolesRepository.UpdateRoles(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("roles/{id}")]
        public IActionResult Delete(string id)
        {
            dynamic response = _rolesRepository.DeleteRoles(id);
            return StatusCode((int)response.statusCode, response);
        }
    }
}
