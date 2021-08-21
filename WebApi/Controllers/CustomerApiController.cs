using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerApiController : ControllerBase
    {
        private readonly ILogger<CustomerApiController> _logger;

        public CustomerApiController(ILogger<CustomerApiController> logger)
        {
            _logger = logger;
            _logger.LogInformation("CustomerApiController");
        }

        [HttpPost("GetCustomerList/{id}")]
        public void GetCustomerList(int id)
        {
            _logger.LogInformation("GetCustomerList "+id);
        }

        [HttpPost("GetCustomerListByJson")]
        public void GetCustomerListByJson([FromBody] JObject param)
        {
            string name = param["Name"].ToString();
            _logger.LogInformation(param.ToString());
        }
    }
}
