using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Obfuscation;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UserService.Functions;
using UserService.Helper.Abstraction;
using UserService.Models;
using UserService.Models.Common;
using UserService.Models.DBModels;
using UserService.Models.ResponseModel;

namespace UserService.Helper.Repository
{
    public class UserIncludedRepository : IUserIncludedRepository
    {
        private readonly userserviceContext _context;
        private readonly AppSettings _appSettings;
        private readonly Dependencies _dependencies;

        public UserIncludedRepository(IOptions<AppSettings> appSettings, userserviceContext context, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _dependencies = dependencies.Value;
        }
        public dynamic GetApplicationIncludedData(List<UsersModel> usersModelList)
        {
            List<ApplicationsModel> lstApplications = new List<ApplicationsModel>();
            foreach (var item in usersModelList)
            {
                foreach (var roleItem in item.Roles)
                {
                    var ApplicationIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(roleItem.ApplicationId), _appSettings.PrimeInverse);
                    var applicationDetails = _context.Applications.Where(x => x.ApplicationId == ApplicationIdDecrypted).FirstOrDefault();
                    if (applicationDetails != null)
                    {
                        ApplicationsModel objModel = new ApplicationsModel()
                        {
                            ApplicationId = ObfuscationClass.EncodeId(applicationDetails.ApplicationId, _appSettings.Prime).ToString(),
                            Name = applicationDetails.Name
                        };
                        lstApplications.Add(objModel);
                    }
                }
            }
            var ApplicationList = lstApplications.GroupBy(x => x.ApplicationId).Select(a => a.First()).ToList();
            return Common.SerializeJsonForIncludedRepo(ApplicationList.Cast<dynamic>().ToList());
        }

        public dynamic GetPrivilegeIncludedData(List<UsersModel> usersModelList)
        {
            List<PrivilegesModel> lstPrivileges = new List<PrivilegesModel>();
            foreach (var item in usersModelList)
            {
                foreach (var roleItem in item.Roles)
                {
                    var PrivilegeIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(roleItem.PrivilegeId), _appSettings.PrimeInverse);
                    var privilegeDetails = _context.Privileges.Where(x => x.PrivilegeId == PrivilegeIdDecrypted).FirstOrDefault();
                    if (privilegeDetails != null)
                    {
                        PrivilegesModel objModel = new PrivilegesModel()
                        {
                            PrivilegeId = ObfuscationClass.EncodeId(privilegeDetails.PrivilegeId, _appSettings.Prime).ToString(),
                            Name = privilegeDetails.Name
                        };
                        lstPrivileges.Add(objModel);
                    }
                }
            }
            var PrivilegeList = lstPrivileges.GroupBy(x => x.PrivilegeId).Select(a => a.First()).ToList();
            return Common.SerializeJsonForIncludedRepo(PrivilegeList.Cast<dynamic>().ToList());
        }
    }
}
