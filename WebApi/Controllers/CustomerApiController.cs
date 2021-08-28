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
using WebApi.Data;
using WebApi.Models;
using System.Globalization;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerApiController : ControllerBase
    {
        private readonly ILogger<CustomerApiController> _logger;
        private CrmDbContext _db;
        public CustomerApiController(ILogger<CustomerApiController> logger, CrmDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost("GetCustomerList")]
        public List<Customer> GetCustomerList([FromBody] JObject param)
        {
            string s = DateTime.Now.ToString();
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            CultureInfo.CurrentCulture = culture;
            s = DateTime.Now.ToString();

            List<Customer> list = new List<Customer>();
            list = (from m in _db.Customers select m).ToList();
            _logger.LogInformation("GetCustomerList Count:" + list.Count);
            return list;
        }

        [HttpPost("AddCustomer")]
        public bool AddCustomer([FromBody] Customer customer)
        {
            bool result = false;
            if (customer != null)
            {
                var dbResult = _db.Customers.Add(customer);
                _db.SaveChanges();
                result = dbResult != null;
            }
            _logger.LogInformation("AddCustomer Result:" + result);
            return result;
        }
    }
}
