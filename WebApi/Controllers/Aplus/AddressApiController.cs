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
    public class AddressApiController : ControllerBase
    {
        private readonly ILogger<AddressApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        public AddressApiController(ILogger<AddressApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetAddressList")]
        public async Task<List<Address>> GetAddressList([FromBody] JObject param)
        {
            List<Address> list = new List<Address>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Address select m).ToListAsync();
                }
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
            _logger.LogInformation("Address: " + JsonConvert.SerializeObject(address));
            bool result = false;
            if (address != null)
            {
                Customer customer = null;
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        customer = context.Customers.Where(o => o.Id == address.CustomerId).FirstOrDefault();
                        address.CreatedDate = DateTime.UtcNow;
                        address.UpdateDate = DateTime.UtcNow;
                        var dbResult = context.Address.Add(address);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if(customer != null)
                {
                    string message = "Address " + address.Name + (result ? " Added" : "Could Not Added") + " To Customer " + customer.Name + " " + customer.Surname;
                    _logger.LogInformation("AddAddress\tParam: " + JsonConvert.SerializeObject(address) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }                
            }
            return result;
        }

        [HttpPost("UpdateAddress")]
        public async Task<bool> UpdateAddress([FromBody] Address address)
        {
            bool result = false;
            if (address != null)
            {
                Customer customer = null;
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        customer = context.Customers.Where(o => o.Id == address.CustomerId).FirstOrDefault();
                        var existing = context.Address.FirstOrDefault(o => o.Id == address.Id);
                        if (existing != null)
                        {
                            existing.Name = address.Name;
                            existing.City = address.City;
                            existing.Country = address.Country;
                            existing.CustomerId = address.CustomerId;
                            existing.Detail = address.Detail;
                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateAddress Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (customer != null)
                {
                    string message = "Address " + address.Name + (result ? " Updated" : "Could Not Updated") + " For Customer " + customer.Name + " " + customer.Surname;
                    _logger.LogInformation("UpdateAddress\tParam: " + JsonConvert.SerializeObject(address) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            _logger.LogInformation("UpdateAddress Result:" + result);
            return result;
        }

        [HttpPost("DeleteAddress")]
        public async Task<bool> DeleteAddress([FromBody] Address address)
        {
            bool result = false;
            if (address != null)
            {
                Customer customer = null;
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        customer = context.Customers.Where(o => o.Id == address.CustomerId).FirstOrDefault();
                        var existing = context.Address.FirstOrDefault(o => o.Id == address.Id);
                        if (existing != null)
                        {
                            context.Address.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteAddress Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (customer != null)
                {
                    string message = "Address " + address.Name + (result ? " Deleted" : "Could Not Deleted") + " From Customer " + customer.Name + " " + customer.Surname;
                    _logger.LogInformation("DeleteAddress\tParam: " + JsonConvert.SerializeObject(address) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            return result;
        }

        [HttpPost("GetAddressListByCustomerId")]
        public async Task<List<Address>> GetAddressListByCustomerId([FromBody] JObject param)
        {
            List<Address> list = new List<Address>();
            _logger.LogInformation("GetAddressListByCustomerId");
            try
            {
                if (param["CustomerId"] != null)
                {
                    int customerId = Convert.ToInt32(param["CustomerId"]);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        list = await (from m in context.Address
                                      where m.CustomerId == customerId
                                      select m).ToListAsync();
                    }
                    _logger.LogInformation("GetAddressList Count:" + list.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            return list;
        }
        private string getUserName()
        {
            return HttpContext.Session.GetString("UserName");
        }
    }
}
