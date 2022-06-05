using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RentReadyWebApi.Model;
using RentReadyWebApi.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Controllers.Aplus;

namespace RentReadyWebApi.Controllers
{
    [ApiController]
    [Route("powerapp/[controller]")]
    public class PowerAppController : BaseApiController
    {       
        private readonly ILogger<PowerAppController> _logger;
        private AccountService accountService = new AccountService();

        public PowerAppController(ILogger<PowerAppController> logger)
        {
            _logger = logger;
        }

        [HttpPost("GetAccountList")]
        public async Task<IEnumerable<Account>> GetAccountList([FromBody] JObject param)
        {
            string text = "";
            if (param["Text"] != null)
            {
                text = param["Text"].ToString();
            }
            return await accountService.GetAccountList(text);            
        }

        [HttpGet("GetAccountListByText")]
        public async Task<IEnumerable<Account>> GetAccounts(string text)
        {            
            return await accountService.GetAccountList(text);
        }

        [HttpGet("GetAccounts")]
        public async Task<IEnumerable<Account>> GetAccounts()
        {
            return await accountService.GetAccountList("");
        }
    }
}
