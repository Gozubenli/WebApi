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
using Newtonsoft.Json;
using WebApi.Utils;
using Microsoft.AspNetCore.Http;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class CustomerApiController : ControllerBase
    {
        private readonly ILogger<CustomerApiController> _logger;
        private DbLogger _dbLogger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        public CustomerApiController(ILogger<CustomerApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetCustomerList")]
        public async Task<List<Customer>> GetCustomerList([FromBody] JObject param)
        {
            List<Customer> list = new List<Customer>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Customers select m).ToListAsync();
                }
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
            _logger.LogInformation("GetCustomerModelList");
            List<UiCustomerModel> resultList = new List<UiCustomerModel>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var list = await (from m in context.Customers
                                      select new
                                      {
                                          customer = m,
                                          //projectList = (from cp in context.Customer_Projects
                                          //               join p in context.Projects on cp.ProjectId equals p.Id
                                          //               where cp.CustomerId == m.Id
                                          //               select p).ToList(),
                                          //addressList = (from a in context.Address
                                          //               where a.CustomerId == m.Id
                                          //               select a).ToList(),
                                      }
                                  ).OrderByDescending(o => o.customer.Id).ToListAsync();
                    foreach (var item in list)
                    {
                        resultList.Add(new UiCustomerModel()
                        {
                            Customer = item.customer,
                            //ProjectList = item.projectList,
                            //AddressList = item.addressList
                        });
                    }
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
                    customer.CreatedDate = DateTime.UtcNow;
                    customer.UpdateDate = DateTime.UtcNow;
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var dbResult = context.Customers.Add(customer);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                string message = "Customer " + customer.Name + " " + customer.Surname + (result ? " Added" : "Could Not Added");
                _logger.LogInformation("AddCustomer\tParam: " + JsonConvert.SerializeObject(customer) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
            return result;
        }

        [HttpPost("UpdateCustomer")]
        public async Task<bool> UpdateCustomer([FromBody] Customer customer)
        {
            _logger.LogInformation("UpdateCustomer: " + JsonConvert.SerializeObject(customer));
            bool result = false;
            if (customer != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Customers.FirstOrDefault(o => o.Id == customer.Id);
                        if (existing != null)
                        {
                            existing.Name = customer.Name;
                            existing.Surname = customer.Surname;
                            existing.UserName = customer.UserName;
                            existing.Email = customer.Email;
                            existing.Telephone = customer.Telephone;
                            existing.RecordBase = customer.RecordBase;
                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateCustomer Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "Customer " + customer.Name + " " + customer.Surname + (result ? " Updated" : "Could Not Updated");
                _logger.LogInformation("UpdateCustomer\tParam: " + JsonConvert.SerializeObject(customer) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
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
                Customer customer = null;
                Project project = null;
                try
                {
                    int custId = Convert.ToInt32(customerId);
                    int projId = Convert.ToInt32(projectId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        customer = context.Customers.Where(o => o.Id == custId).FirstOrDefault();
                        project = context.Projects.Where(o => o.Id == projId).FirstOrDefault();

                        Customer_Project customer_Project = new Customer_Project() { CustomerId = Convert.ToInt32(customerId), ProjectId = Convert.ToInt32(projectId) };
                        var dbResult = context.Add(customer_Project);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (customer != null && project != null)
                {
                    string message = "Project " + project.Name + (result ? " Added" : "Could Not Added") + "  To Customer " + customer.Name + " " + customer.Surname;
                    await _dbLogger.logInfo(message, getUserName());
                    _logger.LogInformation("UpdateCustomer\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        [HttpPost("DeleteCustomerProject")]
        public async Task<bool> DeleteCustomerProject([FromBody] JObject param)
        {
            bool result = false;
            var customerId = param["CustomerId"];
            var projectId = param["ProjectId"];

            if (customerId != null && projectId != null)
            {
                Customer customer = null;
                Project project = null;
                try
                {
                    int custId = Convert.ToInt32(customerId);
                    int projId = Convert.ToInt32(projectId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        customer = context.Customers.Where(o => o.Id == custId).FirstOrDefault();
                        project = context.Projects.Where(o => o.Id == projId).FirstOrDefault();

                        var existing = context.Customer_Projects.FirstOrDefault(o => o.CustomerId == custId && o.ProjectId == projId);
                        if (existing != null)
                        {
                            context.Customer_Projects.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteCustomerProject Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (customer != null && project != null)
                {
                    string message = "Project " + project.Name + (result ? " Deleted" : "Could Not Deleted") + "  From Customer " + customer.Name + " " + customer.Surname;
                    await _dbLogger.logInfo(message, getUserName());
                    _logger.LogInformation("DeleteCustomerProject\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        private string getUserName()
        {
            return HttpContext.Session.GetString("UserName");
        }
    }
}
