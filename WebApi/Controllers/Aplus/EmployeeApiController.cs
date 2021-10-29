﻿using Microsoft.AspNetCore.Mvc;
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
using WebApi.UiModels;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class EmployeeApiController : ControllerBase
    {
        private readonly ILogger<EmployeeApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        public EmployeeApiController(ILogger<EmployeeApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        #region Employees
        [HttpPost("GetEmployeeList")]
        public async Task<List<Employee>> GetEmployeeList([FromBody] JObject param)
        {
            List<Employee> list = new List<Employee>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Employees select m).ToListAsync();
                }
                _logger.LogInformation("GetEmployeeList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("GetEmployeeModelList")]
        public async Task<List<UiEmployeeModel>> GetEmployeeModelList([FromBody] JObject param)
        {
            List<UiEmployeeModel> resultList = new List<UiEmployeeModel>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var employeeList = await (from m in context.Employees select m).ToListAsync();
                    var groupList = await (from m in context.Groups select m).ToListAsync();
                    var employeeGroupList = await (from m in context.Employee_Groups select m).ToListAsync();
                    var workList = await (from m in context.Works select m).ToListAsync();
                    var employeeWorkList = await (from m in context.Employee_Works select m).ToListAsync();

                    foreach (var item in employeeList)
                    {
                        var gl = (from m in employeeGroupList
                                  join n in groupList on m.GroupId equals n.Id
                                  where m.EmployeeId == item.Id
                                  select n).ToList();

                        var wl = (from m in employeeWorkList
                                  join n in workList on m.WorkId equals n.Id
                                  where m.EmployeeId == item.Id
                                  select n).ToList();

                        resultList.Add(new UiEmployeeModel()
                        {
                            Employee = item,
                            GroupList = gl,
                            WorkList = wl
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

        [HttpPost("AddEmployee")]
        public async Task<bool> AddEmployee([FromBody] Employee employee)
        {
            _logger.LogInformation("Employee: " + JsonConvert.SerializeObject(employee));
            bool result = false;
            if (employee != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var dbResult = context.Employees.Add(employee);
                        await context.SaveChangesAsync();
                        var employeeGroup = new Employee_Group() { EmployeeId = employee.Id, GroupId = Defaults.undefinedGroupId };
                        context.Employee_Groups.Add(employeeGroup);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (employee != null)
                {
                    string message = "Employee " + employee.Name + (result ? " Added" : "Could Not Added");
                    _logger.LogInformation("AddEmployee\tParam: " + JsonConvert.SerializeObject(employee) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            return result;
        }

        [HttpPost("UpdateEmployee")]
        public async Task<bool> UpdateEmployee([FromBody] Employee employee)
        {
            bool result = false;
            if (employee != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Employees.FirstOrDefault(o => o.Id == employee.Id);
                        if (existing != null)
                        {
                            existing.Name = employee.Name;
                            existing.Surname = employee.Surname;
                            //existing.UserName = employee.UserName;
                            existing.Email = employee.Email;
                            existing.Telephone = employee.Telephone;
                            existing.TitleId = employee.TitleId;
                            existing.ImageUrl = employee.ImageUrl;
                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateEmployee Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (employee != null)
                {
                    string message = "Employee " + employee.Name + (result ? " Updated" : "Could Not Updated");
                    _logger.LogInformation("UpdateEmployee\tParam: " + JsonConvert.SerializeObject(employee) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            _logger.LogInformation("UpdateEmployee Result:" + result);
            return result;
        }

        [HttpPost("DeleteEmployee")]
        public async Task<bool> DeleteEmployee([FromBody] Employee employee)
        {
            bool result = false;
            if (employee != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Employees.FirstOrDefault(o => o.Id == employee.Id);
                        if (existing != null)
                        {
                            context.Employees.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();

                            var toDeleteList = context.Employee_Groups.Where(o => o.EmployeeId == employee.Id).ToList();
                            if (toDeleteList != null)
                            {
                                foreach (var item in toDeleteList)
                                {
                                    context.Employee_Groups.Remove(item);
                                }
                            }

                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteEmployee Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (employee != null)
                {
                    string message = "Employee " + employee.Name + (result ? " Deleted" : "Could Not Deleted");
                    _logger.LogInformation("DeleteAddress\tParam: " + JsonConvert.SerializeObject(employee) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            return result;
        }

        #endregion

        #region Groups
        [HttpPost("GetGroupList")]
        public async Task<List<Group>> GetGroupList([FromBody] JObject param)
        {
            List<Group> list = new List<Group>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Groups select m).ToListAsync();
                    list = list.OrderBy(o => (o.Id == Defaults.undefinedGroupId)).ToList();
                }
                _logger.LogInformation("GetGroupList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddGroup")]
        public async Task<bool> AddGroup([FromBody] Group group)
        {
            _logger.LogInformation("Group: " + JsonConvert.SerializeObject(group));
            bool result = false;
            if (group != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var dbResult = context.Groups.Add(group);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (group != null)
                {
                    string message = "Group " + group.Name + (result ? " Added" : "Could Not Added");
                    _logger.LogInformation("AddGroup\tParam: " + JsonConvert.SerializeObject(group) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            return result;
        }

        [HttpPost("UpdateGroup")]
        public async Task<bool> UpdateGroup([FromBody] Group group)
        {
            bool result = false;
            if (group != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Groups.FirstOrDefault(o => o.Id == group.Id);
                        if (existing != null)
                        {
                            existing.Name = group.Name;
                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateGroup Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (group != null)
                {
                    string message = "Group " + group.Name + (result ? " Updated" : "Could Not Updated");
                    _logger.LogInformation("UpdateGroup\tParam: " + JsonConvert.SerializeObject(group) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            _logger.LogInformation("UpdateGroup Result:" + result);
            return result;
        }

        [HttpPost("DeleteGroup")]
        public async Task<bool> DeleteGroup([FromBody] Group group)
        {
            bool result = false;
            if (group != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Groups.FirstOrDefault(o => o.Id == group.Id);
                        if (existing != null)
                        {
                            var list = context.Employee_Groups.Where(o => o.GroupId == group.Id).ToList();
                            foreach (var item in list)
                            {
                                item.GroupId = Defaults.undefinedGroupId;
                            }
                            context.Groups.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteGroup Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (group != null)
                {
                    string message = "Group " + group.Name + (result ? " Deleted" : "Could Not Deleted");
                    _logger.LogInformation("DeleteAddress\tParam: " + JsonConvert.SerializeObject(group) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            return result;
        }

        [HttpPost("AddEmployeeGroup")]
        public async Task<bool> AddEmployeeGroup([FromBody] JObject param)
        {
            bool result = false;

            var employeeId = param["EmployeeId"];
            var groupId = param["GroupId"];

            if (employeeId != null && groupId != null)
            {
                Employee employee = null;
                Group group = null;
                try
                {
                    int empId = Convert.ToInt32(employeeId);
                    int groId = Convert.ToInt32(groupId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        employee = context.Employees.Where(o => o.Id == empId).FirstOrDefault();
                        group = context.Groups.Where(o => o.Id == groId).FirstOrDefault();

                        Employee_Group employee_Group = new Employee_Group() { EmployeeId = Convert.ToInt32(employeeId), GroupId = Convert.ToInt32(groupId) };
                        var dbResult = context.Add(employee_Group);

                        //Undefined Grubu silebiliriz, baska grup eklendi
                        var toDelete = (from m in context.Employee_Groups where m.GroupId == 1 && m.EmployeeId == empId select m).FirstOrDefault();
                        if (toDelete != null)
                        {
                            context.Employee_Groups.Remove(toDelete);
                        }

                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (employee != null && group != null)
                {
                    string message = "Group " + group.Name + (result ? " Added" : "Could Not Added") + "  To Employee " + employee.Name + " " + employee.Surname;
                    await _dbLogger.logInfo(message, getUserName());
                    _logger.LogInformation("AddEmployeeGroup\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        /// <summary>
        /// Personelin grubunu 0 yani Undefined yapıyoruz
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("DeleteEmployeeGroup")]
        public async Task<bool> DeleteEmployeeGroup([FromBody] JObject param)
        {
            bool result = false;
            var employeeId = param["EmployeeId"];
            var groupId = param["GroupId"];

            if (employeeId != null && groupId != null)
            {
                Employee employee = null;
                Group group = null;
                try
                {
                    int empId = Convert.ToInt32(employeeId);
                    int grpjId = Convert.ToInt32(groupId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        employee = context.Employees.Where(o => o.Id == empId).FirstOrDefault();
                        group = context.Groups.Where(o => o.Id == grpjId).FirstOrDefault();

                        var existing = context.Employee_Groups.FirstOrDefault(o => o.EmployeeId == empId && o.GroupId == grpjId);
                        if (existing != null)
                        {
                            int count = context.Employee_Groups.Where(o => o.EmployeeId == empId).ToList().Count;
                            if (count > 1)
                            {
                                context.Employee_Groups.Remove(existing);
                            }
                            else
                            {
                                existing.GroupId = Defaults.undefinedGroupId;
                            }

                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteEmployeeGroup Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (employee != null && group != null)
                {
                    string message = "Group " + group.Name + (result ? " Deleted" : "Could Not Deleted") + "  From Employee " + employee.Name + " " + employee.Surname;
                    await _dbLogger.logInfo(message, getUserName());
                    _logger.LogInformation("DeleteEmployeeGroup\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        #endregion

        #region EmployeeWork
        [HttpPost("AddEmployeeWork")]
        public async Task<bool> AddEmployeeWork([FromBody] JObject param)
        {
            bool result = false;

            var employeeId = param["EmployeeId"];
            var workId = param["WorkId"];

            if (employeeId != null && workId != null)
            {
                Employee employee = null;
                Work work = null;
                try
                {
                    int empId = Convert.ToInt32(employeeId);
                    int worId = Convert.ToInt32(workId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        employee = context.Employees.Where(o => o.Id == empId).FirstOrDefault();
                        work = context.Works.Where(o => o.Id == worId).FirstOrDefault();

                        Employee_Work employee_Work = new Employee_Work() { EmployeeId = Convert.ToInt32(employeeId), WorkId = Convert.ToInt32(workId) };
                        var dbResult = context.Add(employee_Work);

                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (employee != null && work != null)
                {
                    string message = "Work " + work.Title + (result ? " Added" : "Could Not Added") + "  To Employee " + employee.Name + " " + employee.Surname;
                    await _dbLogger.logInfo(message, getUserName());
                    _logger.LogInformation("AddEmployeeWork\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        /// <summary>
        /// Personelle ilişkili işi siliyoruz
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("DeleteEmployeeWork")]
        public async Task<bool> DeleteEmployeeWork([FromBody] JObject param)
        {
            bool result = false;
            var employeeId = param["EmployeeId"];
            var workId = param["WorkId"];

            if (employeeId != null && workId != null)
            {
                Employee employee = null;
                Work work = null;
                try
                {
                    int empId = Convert.ToInt32(employeeId);
                    int worId = Convert.ToInt32(workId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        employee = context.Employees.Where(o => o.Id == empId).FirstOrDefault();
                        work = context.Works.Where(o => o.Id == worId).FirstOrDefault();

                        var existing = context.Employee_Works.FirstOrDefault(o => o.EmployeeId == empId && o.WorkId == worId);
                        if (existing != null)
                        {
                            context.Employee_Works.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteEmployeeWork Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (employee != null && work != null)
                {
                    string message = "Work " + work.Title + (result ? " Deleted" : "Could Not Deleted") + "  From Employee " + employee.Name + " " + employee.Surname;
                    await _dbLogger.logInfo(message, getUserName());
                    _logger.LogInformation("DeleteEmployeeWork\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }
        #endregion

        #region Title
        [HttpPost("GetTitleList")]
        public async Task<List<Title>> GetTitleList([FromBody] JObject param)
        {
            List<Title> list = new List<Title>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Titles select m).ToListAsync();
                }
                _logger.LogInformation("GetTitleList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddTitle")]
        public async Task<bool> AddTitle([FromBody] Title title)
        {
            _logger.LogInformation("Title: " + JsonConvert.SerializeObject(title));
            bool result = false;
            if (title != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var dbResult = context.Titles.Add(title);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (title != null)
                {
                    string message = "Title " + title.Name + (result ? " Added" : "Could Not Added");
                    _logger.LogInformation("AddTitle\tParam: " + JsonConvert.SerializeObject(title) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            return result;
        }

        [HttpPost("UpdateTitle")]
        public async Task<bool> UpdateTitle([FromBody] Title title)
        {
            bool result = false;
            if (title != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Titles.FirstOrDefault(o => o.Id == title.Id);
                        if (existing != null)
                        {
                            existing.Name = title.Name;
                            existing.UpdateDate = DateTime.UtcNow;
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateTitle Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (title != null)
                {
                    string message = "Title " + title.Name + (result ? " Updated" : "Could Not Updated");
                    _logger.LogInformation("UpdateTitle\tParam: " + JsonConvert.SerializeObject(title) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            _logger.LogInformation("UpdateTitle Result:" + result);
            return result;
        }

        [HttpPost("DeleteTitle")]
        public async Task<bool> DeleteTitle([FromBody] Title title)
        {
            bool result = false;
            if (title != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Titles.FirstOrDefault(o => o.Id == title.Id);
                        if (existing != null)
                        {
                            var list = context.Employees.Where(o => o.TitleId == title.Id).ToList();
                            foreach (var item in list)
                            {
                                item.TitleId = Defaults.undefinedTitleId;
                            }
                            context.Titles.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteTitle Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (title != null)
                {
                    string message = "Title " + title.Name + (result ? " Deleted" : "Could Not Deleted");
                    _logger.LogInformation("DeleteAddress\tParam: " + JsonConvert.SerializeObject(title) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, getUserName());
                }
            }
            return result;
        }

        #endregion

        private string getUserName()
        {
            return HttpContext.Session.GetString("UserName");
        }
    }
}
