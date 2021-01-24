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
    public class PrivilegeController : ControllerBase
    {
        private readonly IPrivilegesRepository _privilegesRepository;
        public PrivilegeController(IPrivilegesRepository privilegesRepository)
        {
            _privilegesRepository = privilegesRepository;
        }

        [HttpPost]
        [Route("privileges")]
        public IActionResult Post(PrivilegesModel model)
        {
            dynamic response = _privilegesRepository.PostPrivilege(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("privileges")]
        public IActionResult Put(PrivilegesModel model)
        {
            dynamic response = _privilegesRepository.PutPrivilege(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpGet]
        [Route("privileges/{privilegeId?}")]
        public IActionResult Get(string privilegeId, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _privilegesRepository.GetPrivilege(privilegeId, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("privileges/{id}")]
        public IActionResult Delete(int id)
        {
            dynamic response = _privilegesRepository.DeletePrivilege(id);
            return StatusCode((int)response.statusCode, response);
        }



    }
}
