using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using Microsoft.EntityFrameworkCore;
using WebApi.DbModels;
using Newtonsoft.Json;
using WebApi.Utils;
using Microsoft.AspNetCore.Http;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class ProjectApiController : ControllerBase
    {
        private readonly ILogger<ProjectApiController> _logger;
        private CrmDbContext _db;
        private DbLogger _dbLogger;
        public ProjectApiController(ILogger<ProjectApiController> logger, CrmDbContext db)
        {
            _logger = logger;
            _db = db;
            _dbLogger = new DbLogger(_db);
        }

        [HttpPost("GetProjectList")]
        public async Task<List<Project>> GetProjectList([FromBody] JObject param)
        {
            Dictionary<string, string> a = new Dictionary<string, string>();
            List<Project> list = new List<Project>();
            try
            {
                list = await (from m in _db.Projects select m).ToListAsync();
                _logger.LogInformation("GetProjectList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddProject")]
        public async Task<bool> AddProject([FromBody] Project project)
        {
            bool result = false;
            if (project != null)
            {
                try
                {
                    var dbResult = _db.Projects.Add(project);
                    await _db.SaveChangesAsync();
                    result = dbResult != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "Project " + project.Name + (result ? " Added" : "Could Not Added");
                _logger.LogInformation("AddProject\tParam: " + JsonConvert.SerializeObject(project) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
            return result;
        }

        [HttpPost("GetProjectListByCustomerId")]
        public async Task<List<Project>> GetProjectListByCustomerId([FromBody] JObject param)
        {
            List<Project> list = new List<Project>();
            
            try
            {
                if(param["CustomerId"] != null)
                {
                    int customerId = Convert.ToInt32(param["CustomerId"]);
                    list = await (from m in _db.Projects
                                  join cp in _db.Customer_Projects on m.Id equals cp.ProjectId
                                  where cp.CustomerId == customerId select m).ToListAsync();                    
                }
                _logger.LogInformation("GetProjectListByCustomerId\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return list;
        }

        [HttpPost("DeleteProject")]
        public async Task<bool> DeleteProject([FromBody] Project project)
        {
            bool result = false;
            if (project != null)
            {
                try
                {
                    var existing = _db.Projects.FirstOrDefault(o => o.Id == project.Id);
                    if (existing != null)
                    {
                        _db.Projects.Remove(existing);
                        int dbResult = await _db.SaveChangesAsync();
                        result = dbResult > 0;
                    }
                    else
                    {
                        _logger.LogError("DeleteProject Not Found");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "Project " + project.Name + (result ? " Deleted" : "Could Not Deleted");
                _logger.LogInformation("DeleteProject\tParam: " + JsonConvert.SerializeObject(project) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
            return result;
        }
        private string getUserName()
        {
            return HttpContext.Session.GetString("UserName");
        }
    }
}
