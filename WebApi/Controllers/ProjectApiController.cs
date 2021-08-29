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

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectApiController : ControllerBase
    {
        private readonly ILogger<ProjectApiController> _logger;
        private CrmDbContext _db;
        public ProjectApiController(ILogger<ProjectApiController> logger, CrmDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost("GetProjectList")]
        public async Task<List<Project>> GetProjectList([FromBody] JObject param)
        {
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
            }
            _logger.LogInformation("AddProject Result:" + result);
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
                    _logger.LogInformation("GetProjectList Count:" + list.Count);
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }
    }
}
