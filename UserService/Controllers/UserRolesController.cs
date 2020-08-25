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
    public class RolesController : BaseController
    {
        private readonly IRolesRepository _rolesRepository;
        public RolesController(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }

        [HttpGet]
        [Route("roles/{id=0}")]
        public IActionResult Get(int id, [FromQuery] Pagination pageInfo)
        {
            RolesGetResponse response = new RolesGetResponse();
            response = _rolesRepository.GetRoles(id, pageInfo);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("roles")]
        public IActionResult Post(RolesModel model)
        {
            RolesResponse response = new RolesResponse();
            if (ModelState.IsValid)
                response = _rolesRepository.InsertRoles(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPut]
        [Route("roles")]
        public IActionResult Put(RolesModel model)
        {
            RolesResponse response = new RolesResponse();
            if (ModelState.IsValid)
                response = _rolesRepository.UpdateRoles(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpDelete]
        [Route("roles/{id}")]
        public IActionResult Delete(int id)
        {
            RolesResponse response = new RolesResponse();
            if (ModelState.IsValid)
                response = _rolesRepository.DeleteRoles(id);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}
