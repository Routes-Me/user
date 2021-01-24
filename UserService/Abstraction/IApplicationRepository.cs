using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;
using UserService.Models.ResponseModel;

namespace UserService.Abstraction
{
    public interface IApplicationRepository
    {
        dynamic PostApplication(ApplicationsModel model);
        dynamic PutApplication(ApplicationsModel model);
        dynamic GetApplication(string applicationId, Pagination pageInfo);
        dynamic DeleteApplication(int id);
    }
}
