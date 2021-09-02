using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;
using System.Globalization;
using System.Net.Http;
using System.Net;
using Microsoft.EntityFrameworkCore;
using WebApi.DbModels;
using WebApi.UiModels;

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
        public async Task<List<Customer>> GetCustomerList([FromBody] JObject param)
        {
            List<Customer> list = new List<Customer>();
            try
            {
                list = await (from m in _db.Customers select m).ToListAsync();
                _logger.LogInformation("GetCustomerList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("GetCustomerModelList")]
        public async Task<List<UiCustomerModel>> GetCustomerModelList([FromBody] JObject param)
        {
            List<UiCustomerModel> resultList = new List<UiCustomerModel>();

            try
            {
                var list = await (from m in _db.Customers
                                  select new
                                  {
                                      customer = m,
                                      projectList = (from cp in _db.Customer_Projects
                                                     join p in _db.Projects on cp.ProjectId equals p.Id
                                                     where cp.CustomerId == m.Id
                                                     select p).ToList(),
                                      addressList = (from a in _db.Address
                                                     where a.CustomerId == m.Id
                                                     select a).ToList(),
                                  }
                                  ).ToListAsync();

                foreach (var item in list)
                {
                    resultList.Add(new UiCustomerModel()
                    {
                        Customer = item.customer,
                        ProjectList = item.projectList,
                        AddressList = item.addressList
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return resultList;
        }

        [HttpPost("AddCustomer")]
        public async Task<bool> AddCustomer([FromBody] Customer customer)
        {
            bool result = false;
            if (customer != null)
            {
                try
                {
                    var dbResult = _db.Customers.Add(customer);
                    await _db.SaveChangesAsync();
                    result = dbResult != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            _logger.LogInformation("AddCustomer Result:" + result);
            return result;
        }

        [HttpPost("AddCustomerProject")]
        public async Task<bool> AddCustomerProject([FromBody] JObject param)
        {
            bool result = false;

            var customerId = param["CustomerId"];
            var projectId = param["ProjectId"];

            if (customerId != null && projectId != null)
            {
                try
                {
                    Customer_Project customer_Project = new Customer_Project() { CustomerId = Convert.ToInt32(customerId), ProjectId = Convert.ToInt32(projectId) };
                    var dbResult = _db.Add(customer_Project);
                    await _db.SaveChangesAsync();
                    result = dbResult != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            _logger.LogInformation("AddCustomerProject Result:" + result);
            return result;
        }       
    }
}
