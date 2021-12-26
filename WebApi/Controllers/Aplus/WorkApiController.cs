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
using WebApi.Controllers.Aplus;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class WorkApiController : BaseApiController
    {
        private readonly ILogger<WorkApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        public WorkApiController(ILogger<WorkApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetWorkList")]
        public async Task<List<Work>> GetWorkList([FromBody] JObject param)
        {
            List<Work> list = new List<Work>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Works select m).ToListAsync();
                }
                _logger.LogInformation("GetWorkList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddWork")]
        public async Task<bool> AddWork([FromBody] Work work)
        {
            _logger.LogInformation("Work: " + JsonConvert.SerializeObject(work));
            bool result = false;
            if (work != null)
            {
                Customer customer = null;
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        customer = context.Customers.Where(o => o.Id == work.CustomerId).FirstOrDefault();
                        work.CreatedDate = DateTime.UtcNow;
                        work.UpdateDate = DateTime.UtcNow;
                        var dbResult = context.Works.Add(work);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }

                    HandleRecurringWork(work);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (customer != null)
                {
                    string message = "Work " + work.Title + (result ? " Added" : "Could Not Added") + " To Customer " + customer.Name + " " + customer.Surname;
                    _logger.LogInformation("AddWork\tParam: " + JsonConvert.SerializeObject(work) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, GetUserName());
                }
            }
            return result;
        }

        [HttpPost("UpdateWork")]
        public async Task<bool> UpdateWork([FromBody] Work work)
        {
            bool result = false;
            if (work != null)
            {
                Customer customer = null;
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        customer = context.Customers.Where(o => o.Id == work.CustomerId).FirstOrDefault();
                        var existing = context.Works.FirstOrDefault(o => o.Id == work.Id);
                        if (existing != null)
                        {
                            existing.Title = work.Title;
                            existing.Detail = work.Detail;
                            existing.CustomerId = work.CustomerId;
                            existing.CategoryId = work.CategoryId;
                            existing.EmergencyStatus = work.EmergencyStatus;
                            existing.ProjectId = work.ProjectId;
                            existing.WorkStatus = work.WorkStatus;
                            existing.WorkTime = work.WorkTime;
                            existing.WorkType = work.WorkType;
                            existing.WorkTime = work.WorkTime;
                            existing.PlannedDateTime = work.PlannedDateTime;
                            existing.PlannedHours = work.PlannedHours;
                            existing.WorkPeriodType = work.WorkPeriodType;
                            existing.WorkPeriodNumber = work.WorkPeriodNumber;
                            existing.WorkPeriodEndDate = work.WorkPeriodEndDate;
                            existing.WorkPeriodRecurringDays = work.WorkPeriodRecurringDays;
                            existing.WorkPeriodRecurringType = work.WorkPeriodRecurringType;
                            existing.WorkPeriodRootId = work.WorkPeriodRootId;

                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateWork Not Found");
                        }
                    }
                    HandleRecurringWork(work);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (customer != null)
                {
                    string message = "Work " + work.Title + (result ? " Updated" : "Could Not Updated") + " For Customer " + customer.Name + " " + customer.Surname;
                    _logger.LogInformation("UpdateWork\tParam: " + JsonConvert.SerializeObject(work) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, GetUserName());
                }
            }
            _logger.LogInformation("UpdateWork Result:" + result);
            return result;
        }

        [HttpPost("DeleteWork")]
        public async Task<bool> DeleteWork([FromBody] Work work)
        {
            bool result = false;
            if (work != null)
            {
                Customer customer = null;
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        customer = context.Customers.Where(o => o.Id == work.CustomerId).FirstOrDefault();
                        var existing = context.Works.FirstOrDefault(o => o.Id == work.Id);
                        if (existing != null)
                        {
                            context.Works.Remove(existing);

                            var subList = context.Works.Where(o => o.WorkPeriodRootId == work.Id).ToList();
                            context.Works.RemoveRange(subList);

                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteWork Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (customer != null)
                {
                    string message = "Work " + work.Title + (result ? " Deleted" : "Could Not Deleted") + " From Customer " + customer.Name + " " + customer.Surname;
                    _logger.LogInformation("DeleteWork\tParam: " + JsonConvert.SerializeObject(work) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, GetUserName());
                }
            }
            return result;
        }

        [HttpPost("GetCustomerWorkList")]
        public async Task<List<Work>> GetCustomerWorkList([FromBody] JObject param)
        {
            List<Work> list = new List<Work>();
            try
            {
                if (param["CustomerId"] != null)
                {
                    int customerId = Convert.ToInt32(param["CustomerId"]);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        list = await (from m in context.Works
                                      where m.CustomerId == customerId
                                      select m).ToListAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            return list;
        }

        [HttpPost("GetCategoryWorkList")]
        public async Task<List<Work>> GetCategoryWorkList([FromBody] JObject param)
        {
            List<Work> list = new List<Work>();
            try
            {
                if (param["CategoryId"] != null)
                {
                    int categoryId = Convert.ToInt32(param["CategoryId"]);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        list = await (from m in context.Works where m.CategoryId == categoryId select m).ToListAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            return list;
        }

        public async void HandleRecurringWork(Work work)
        {
            try
            {
                if (work.WorkType == WorkType.Recurring && work.WorkPeriodRootId == 0)
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var subRecurringWorkList = context.Works.Where(o => o.WorkPeriodRootId == work.Id).ToList();
                        if (subRecurringWorkList.Count > 0)
                        {
                            context.RemoveRange(subRecurringWorkList);
                        }

                        DateTime nextWorkDate = work.CreatedDate;

                        do
                        {
                            if (work.WorkPeriodRecurringType != null && work.WorkPeriodRecurringType == 0)
                            {
                                if (work.WorkPeriodType == WorkPeriod.Days)
                                {
                                    nextWorkDate = nextWorkDate.AddDays(work.WorkPeriodNumber??0);
                                }
                                else if (work.WorkPeriodType == WorkPeriod.Weeks)
                                {
                                    nextWorkDate = nextWorkDate.AddDays(work.WorkPeriodNumber ?? 0 * 7);
                                }
                                else if (work.WorkPeriodType == WorkPeriod.Months)
                                {
                                    nextWorkDate = nextWorkDate.AddMonths(work.WorkPeriodNumber ?? 0);
                                }
                                else if (work.WorkPeriodType == WorkPeriod.Years)
                                {
                                    nextWorkDate = nextWorkDate.AddYears(work.WorkPeriodNumber ?? 0);
                                }
                                else
                                {
                                    nextWorkDate = work.WorkPeriodEndDate ?? DateTime.Now.AddDays(30);
                                }

                                Work subWork = new Work()
                                {
                                    CategoryId = work.CategoryId,
                                    CustomerId = work.CustomerId,
                                    Detail = work.Detail,
                                    EmergencyStatus = work.EmergencyStatus,
                                    PlannedDateTime = nextWorkDate,
                                    PlannedHours = work.PlannedHours,
                                    ProjectId = work.ProjectId,
                                    Title = work.Title,
                                    WorkStatus = WorkStatus.New,
                                    WorkTime = work.WorkTime,
                                    WorkType = WorkType.Regular,
                                    WorkPeriodRootId = work.Id
                                };

                                context.Works.Add(subWork);

                                _logger.LogInformation("Add Recurring Sub Work \tParam: " + JsonConvert.SerializeObject(subWork));
                            }
                            else
                            {
                                var days = work.WorkPeriodRecurringDays.Split(";").ToList();
                                if(days!= null && days.Count>0)
                                {
                                    foreach (var day in days)
                                    {
                                        var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day, true);
                                        var allDays= DateTime.Today.GetWeekdayInRange(work.WorkPeriodEndDate?? DateTime.Now.AddDays(30), dayOfWeek);
                                        foreach (var item in allDays)
                                        {
                                            nextWorkDate = item;
                                            Work subWork = new Work()
                                            {
                                                CategoryId = work.CategoryId,
                                                CustomerId = work.CustomerId,
                                                Detail = work.Detail,
                                                EmergencyStatus = work.EmergencyStatus,
                                                PlannedDateTime = nextWorkDate,
                                                PlannedHours = work.PlannedHours,
                                                ProjectId = work.ProjectId,
                                                Title = work.Title,
                                                WorkStatus = WorkStatus.New,
                                                WorkTime = work.WorkTime,
                                                WorkType = WorkType.Regular,
                                                WorkPeriodRootId = work.Id
                                            };

                                            context.Works.Add(subWork);

                                            _logger.LogInformation("Add Recurring Sub Work \tParam: " + JsonConvert.SerializeObject(subWork));
                                        }
                                    }
                                }
                            }

                            
                        }
                        while (nextWorkDate < work.WorkPeriodEndDate);

                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
        }

    }
}
