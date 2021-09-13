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
        private DbLogger _dbLogger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        public ProjectApiController(ILogger<ProjectApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetProjectList")]
        public async Task<List<Project>> GetProjectList([FromBody] JObject param)
        {
            Dictionary<string, string> a = new Dictionary<string, string>();
            List<Project> list = new List<Project>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Projects select m).ToListAsync();
                }
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
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var dbResult = context.Projects.Add(project);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
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
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        list = await (from m in context.Projects
                                      join cp in context.Customer_Projects on m.Id equals cp.ProjectId
                                      where cp.CustomerId == customerId
                                      select m).ToListAsync();
                    }
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
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Projects.FirstOrDefault(o => o.Id == project.Id);
                        if (existing != null)
                        {
                            context.Projects.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteProject Not Found");
                        }
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
