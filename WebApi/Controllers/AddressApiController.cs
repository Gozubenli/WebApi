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
    public class AddressApiController : ControllerBase
    {
        private readonly ILogger<AddressApiController> _logger;
        private CrmDbContext _db;
        public AddressApiController(ILogger<AddressApiController> logger, CrmDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost("GetAddressList")]
        public async Task<List<Address>> GetAddressList([FromBody] JObject param)
        {
            List<Address> list = new List<Address>();
            try
            {
                list = await (from m in _db.Address select m).ToListAsync();
                _logger.LogInformation("GetAddressList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddAddress")]
        public async Task<bool> AddAddress([FromBody] Address address)
        {
            bool result = false;
            if (address != null)
            {
                try
                {
                    var dbResult = _db.Address.Add(address);
                    await _db.SaveChangesAsync();
                    result = dbResult != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            _logger.LogInformation("AddAddress Result:" + result);
            return result;
        }

        [HttpPost("GetAddressListByCustomerId")]
        public async Task<List<Address>> GetAddressListByCustomerId([FromBody] JObject param)
        {
            List<Address> list = new List<Address>();
            
            try
            {
                if(param["CustomerId"] != null)
                {
                    int customerId = Convert.ToInt32(param["CustomerId"]);
                    list = await (from m in _db.Address
                                  where m.CustomerId == customerId select m).ToListAsync();
                    _logger.LogInformation("GetAddressList Count:" + list.Count);
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
