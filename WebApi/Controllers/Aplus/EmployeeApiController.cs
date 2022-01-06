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
using WebApi.UiModels;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using WebApi.Controllers.Aplus;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class EmployeeApiController : BaseApiController
    {
        private readonly ILogger<EmployeeApiController> _logger;
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        private string imageFolder = "images/aplus/";

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
            DateTime startDate = DateTime.Today.AddDays(-30);
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var employeeList = await (from m in context.Employees select m).ToListAsync();
                    var groupList = await (from m in context.Groups select m).ToListAsync();
                    var employeeGroupList = await (from m in context.Employee_Groups select m).ToListAsync();
                    var workList = await (from m in context.Works orderby m.Id select m).ToListAsync();
                    var employeeWorkList = await (from m in context.Employee_Works select m).ToListAsync();
                    var roleList = await (from m in context.Roles orderby m.Id select m).ToListAsync();
                    var employeeRoleList = await (from m in context.Employee_Roles select m).ToListAsync();
                    var locationList = await (from m in context.Locations where m.CreatedDate > startDate select m).ToListAsync();

                    foreach (var employee in employeeList)
                    {
                        employee.Password = "";
                        var gl = (from m in employeeGroupList
                                  join n in groupList on m.GroupId equals n.Id
                                  where m.EmployeeId == employee.Id
                                  select n).ToList();

                        var wl = (from m in employeeWorkList
                                  join n in workList on m.WorkId equals n.Id
                                  where m.EmployeeId == employee.Id
                                  select n).ToList();

                        var rl = (from m in employeeRoleList
                                  join n in roleList on m.RoleId equals n.Id
                                  where m.EmployeeId == employee.Id
                                  select n).ToList();

                        var ll = (from m in locationList
                                  where m.EmployeeId == employee.Id
                                  orderby m.Id descending
                                  select m).FirstOrDefault();


                        resultList.Add(new UiEmployeeModel()
                        {
                            Employee = employee,
                            GroupList = gl,
                            WorkList = wl,
                            RoleList = rl,
                            LastLocation = ll,
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

        [HttpPost("GetEmployeePlanList")]
        public async Task<List<UiEmployeePlan>> GetEmployeePlanList([FromBody] JObject param)
        {
            List<UiEmployeePlan> list = new List<UiEmployeePlan>();
            try
            {
                if (param["PlannedDate"] != null)
                {                   
                    DateTime plannedDate = DateTime.ParseExact(param["PlannedDate"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    plannedDate = plannedDate.Date;
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var employeeList = await (from m in context.Employees select m).ToListAsync();

                        var workList = await (from m in context.Works
                                              join n in context.Employee_Works on m.Id equals n.WorkId
                                              join e in context.Employees on n.EmployeeId equals e.Id
                                              where m.PlannedDateTime != null && m.PlannedDateTime.Value.Date == plannedDate orderby e.Id orderby m.PlannedDateTime 
                                              select new { work = m, employee = e }).ToListAsync();
                                                
                        int startHour = 7;
                        foreach (var emp in employeeList)
                        {
                            UiEmployeePlan model = new UiEmployeePlan();
                            model.Employee = emp;
                            Plan[] ocList = new Plan[15];
                            var wl = workList.Where(o => o.employee.Id == emp.Id).OrderBy(o=> o.work.PlannedDateTime).ToList();
                            for (int hour= startHour; hour < startHour+15; hour++)
                            {
                                var ew = wl.FirstOrDefault(o => o.work.PlannedDateTime.Value.Hour == hour || (o.work.PlannedDateTime.Value.Hour < hour && o.work.PlannedDateTime.Value.Hour+ o.work.PlannedHours > hour));
                                int index = hour - startHour;
                                int? wId = ew != null ? ew.work.Id : null;
                                int? custId = ew != null ? ew.work.CustomerId : null;
                                ocList[index] = new Plan() { index = index, workTime = (WorkTime)index, status = (ew!=null), workId = wId, customerId = custId};
                            }
                            model.PlanList = ocList.ToList();
                            list.Add(model);
                        }
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

        private async Task<UiEmployeeModel> GetEmployeeModel(string email, string password)
        {
            UiEmployeeModel result = new UiEmployeeModel();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var employee = await (from m in context.Employees where m.Email == email.ToString() && m.Password == password.ToString() select m).FirstOrDefaultAsync();

                    if (employee != null)
                    {
                        var groupList = await (from m in context.Groups select m).ToListAsync();
                        var employeeGroupList = await (from m in context.Employee_Groups where m.EmployeeId == employee.Id select m).ToListAsync();
                        var workList = await (from m in context.Works orderby m.Id select m).ToListAsync();
                        var employeeWorkList = await (from m in context.Employee_Works where m.EmployeeId == employee.Id select m).ToListAsync();
                        var roleList = await (from m in context.Roles orderby m.Id select m).ToListAsync();
                        var employeeRoleList = await (from m in context.Employee_Roles where m.EmployeeId == employee.Id select m).ToListAsync();
                        employee.Password = "";

                        var gl = (from m in employeeGroupList
                                  join n in groupList on m.GroupId equals n.Id
                                  where m.EmployeeId == employee.Id
                                  select n).ToList();

                        var wl = (from m in employeeWorkList
                                  join n in workList on m.WorkId equals n.Id
                                  where m.EmployeeId == employee.Id
                                  select n).ToList();

                        var rl = (from m in employeeRoleList
                                  join n in roleList on m.RoleId equals n.Id
                                  where m.EmployeeId == employee.Id
                                  select n).ToList();


                        result = new UiEmployeeModel()
                        {
                            Employee = employee,
                            GroupList = gl,
                            WorkList = wl,
                            RoleList = rl
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return result;
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                            existing.Password = employee.Password;
                            existing.Email = employee.Email;
                            existing.Telephone = employee.Telephone;
                            existing.TitleId = employee.TitleId;
                            existing.ImageName = employee.ImageName;
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                    await _dbLogger.logInfo(message, GetUserName());
                }
            }
            return result;
        }

        [HttpPost("UploadEmployeeImage")]
        public async Task<IActionResult> UploadEmployeeImage(IFormFile file)
        {
            try
            {
                _logger.LogInformation("FileName:" + file.FileName + " Length:" + file.Length, GetUserName());
                string path = Path.Combine(Directory.GetCurrentDirectory(), "../" + imageFolder + "personel/", file.FileName);
                var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                using (var context = _contextFactory.CreateDbContext())
                {
                    int employeeId = Convert.ToInt32(file.FileName.Split(".")[0]);
                    var existing = context.Employees.FirstOrDefault(o => o.Id == employeeId);
                    if (existing != null)
                    {
                        existing.ImageName = file.FileName;
                        existing.UpdateDate = DateTime.UtcNow;
                        int dbResult = await context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogError("UpdateEmployee Not Found");
                    }
                }

                return Ok(new { length = file.Length, name = file.FileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [HttpGet("GetEmployeeImage/{employeeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmployeeImage(int employeeId)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "../" + imageFolder + "personel/", "default.png");
            FileStream image = System.IO.File.OpenRead(path);
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var existing = context.Employees.FirstOrDefault(o => o.Id == employeeId);
                    if (existing != null)
                    {
                        path = Path.Combine(Directory.GetCurrentDirectory(), "../" + imageFolder + "personel/", existing.ImageName);
                        image = System.IO.File.OpenRead(path);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return File(image, "image/" + Path.GetExtension(path).Replace(".", ""));
        }

        [HttpPost("UploadCategoryImage")]
        public async Task<IActionResult> UploadCategoryImage(IFormFile file)
        {
            try
            {
                _logger.LogInformation("FileName:" + file.FileName + " Length:" + file.Length, GetUserName());
                string path = Path.Combine(Directory.GetCurrentDirectory(), "../" + imageFolder + "category/", file.FileName);
                var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                using (var context = _contextFactory.CreateDbContext())
                {
                    int categgoryId = Convert.ToInt32(file.FileName.Split(".")[0]);
                    var existing = context.Categories.FirstOrDefault(o => o.Id == categgoryId);
                    if (existing != null)
                    {
                        existing.Image = file.FileName;
                        existing.UpdateDate = DateTime.UtcNow;
                        int dbResult = await context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogError("UploadCategoryImage Not Found");
                    }
                }

                return Ok(new { length = file.Length, name = file.FileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [HttpGet("GetCategoryImage/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryImage(int categoryId)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "../" + imageFolder + "category/", "default.png");
            FileStream image = System.IO.File.OpenRead(path);
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var existing = context.Categories.FirstOrDefault(o => o.Id == categoryId);
                    if (existing != null)
                    {
                        path = Path.Combine(Directory.GetCurrentDirectory(), "../" + imageFolder + "category/", existing.Image);
                        if (System.IO.File.Exists(path))
                        {
                            image = System.IO.File.OpenRead(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return File(image, "image/" + Path.GetExtension(path).Replace(".", ""));
        }

        [HttpPost("Login")]
        public async Task<UiEmployeeModel> Login([FromBody] JObject param)
        {
            var email = param["Email"];
            var password = param["Password"];

            UiEmployeeModel user = null;
            try
            {
                if (email != null && password != null)
                {
                    user = await GetEmployeeModel(email.ToString(), password.ToString());

                    if (user != null && user.Employee != null)
                    {
                        user.Employee.Password = "";
                        _logger.LogInformation("Login " + email.ToString());

                        string message = "Employee " + email + " Logged In";
                        _logger.LogInformation("Login\tParam: " + JsonConvert.SerializeObject(user) + "\tLogged In");
                        await _dbLogger.logInfo(message, GetUserName());
                    }
                    else
                    {
                        string message = "Employee " + email + " Could Not Logged In";
                        _logger.LogInformation("Login\tParam: " + JsonConvert.SerializeObject(param) + "\tCould Not Logged In");
                        await _dbLogger.logInfo(message, email.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }

            return user;
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                    await _dbLogger.logInfo(message, GetUserName());
                    _logger.LogInformation("DeleteEmployeeGroup\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        #endregion

        #region EmployeeWork

        //[HttpPost("UpdateEmployeeWork")]
        //public async Task<bool> UpdateEmployeeWork([FromBody] List<Employee_Work> employee_WorkList)
        //{
        //    bool result = false;

        //    if (employee_WorkList != null && employee_WorkList.Count > 0 != null)
        //    {
        //        Employee employee = null;
        //        Work work = null;
        //        try
        //        {
        //            int empId = Convert.ToInt32(employeeId);
        //            int worId = Convert.ToInt32(workId);
        //            using (var context = _contextFactory.CreateDbContext())
        //            {
        //                employee = context.Employees.Where(o => o.Id == empId).FirstOrDefault();
        //                work = context.Works.Where(o => o.Id == worId).FirstOrDefault();

        //                Employee_Work employee_Work = new Employee_Work() { EmployeeId = Convert.ToInt32(employeeId), WorkId = Convert.ToInt32(workId) };
        //                var dbResult = context.Add(employee_Work);

        //                await context.SaveChangesAsync();
        //                result = dbResult != null;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex.Message);
        //        }

        //        if (employee != null && work != null)
        //        {
        //            string message = "Work " + work.Title + (result ? " Added" : "Could Not Added") + "  To Employee " + employee.Name + " " + employee.Surname;
        //            await _dbLogger.logInfo(message, GetUserName());
        //            _logger.LogInformation("AddEmployeeWork\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
        //        }
        //    }
        //    return result;
        //}

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
                    await _dbLogger.logInfo(message, GetUserName());
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

                        var existingList = context.Employee_Works.Where(o => o.EmployeeId == empId && o.WorkId == worId).ToList();
                        if (existingList != null && existingList.Count > 0)
                        {
                            foreach (var existing in existingList)
                            {
                                context.Employee_Works.Remove(existing);
                            }
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                    await _dbLogger.logInfo(message, GetUserName());
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
                    await _dbLogger.logInfo(message, GetUserName());
                }
            }
            return result;
        }

        #endregion

        #region Role

        [HttpPost("GetRoleList")]
        public async Task<List<Role>> GetRoleList([FromBody] JObject param)
        {
            List<Role> list = new List<Role>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Roles select m).ToListAsync();
                }
                _logger.LogInformation("GetRoleList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddEmployeeRole")]
        public async Task<bool> AddEmployeeRole([FromBody] JObject param)
        {
            bool result = false;

            var employeeId = param["EmployeeId"];
            var roleId = param["RoleId"];

            if (employeeId != null && roleId != null)
            {
                Employee employee = null;
                Role role = null;
                try
                {
                    int empId = Convert.ToInt32(employeeId);
                    int groId = Convert.ToInt32(roleId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        employee = context.Employees.Where(o => o.Id == empId).FirstOrDefault();
                        role = context.Roles.Where(o => o.Id == groId).FirstOrDefault();

                        Employee_Role employee_Role = new Employee_Role() { EmployeeId = Convert.ToInt32(employeeId), RoleId = Convert.ToInt32(roleId) };
                        var dbResult = context.Add(employee_Role);

                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (employee != null && role != null)
                {
                    string message = "Role " + role.Name + (result ? " Added" : "Could Not Added") + "  To Employee " + employee.Name + " " + employee.Surname;
                    await _dbLogger.logInfo(message, GetUserName());
                    _logger.LogInformation("AddEmployeeRole\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        /// <summary>
        /// Personelin grubunu 0 yani Undefined yapıyoruz
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("DeleteEmployeeRole")]
        public async Task<bool> DeleteEmployeeRole([FromBody] JObject param)
        {
            bool result = false;
            var employeeId = param["EmployeeId"];
            var roleId = param["RoleId"];

            if (employeeId != null && roleId != null)
            {
                Employee employee = null;
                Role role = null;
                try
                {
                    int empId = Convert.ToInt32(employeeId);
                    int grpjId = Convert.ToInt32(roleId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        employee = context.Employees.Where(o => o.Id == empId).FirstOrDefault();
                        role = context.Roles.Where(o => o.Id == grpjId).FirstOrDefault();

                        var existing = context.Employee_Roles.FirstOrDefault(o => o.EmployeeId == empId && o.RoleId == grpjId);
                        if (existing != null)
                        {
                            int count = context.Employee_Roles.Where(o => o.EmployeeId == empId).ToList().Count;
                            if (count > 0)
                            {
                                context.Employee_Roles.Remove(existing);
                            }

                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteEmployeeRole Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (employee != null && role != null)
                {
                    string message = "Role " + role.Name + (result ? " Deleted" : "Could Not Deleted") + "  From Employee " + employee.Name + " " + employee.Surname;
                    await _dbLogger.logInfo(message, GetUserName());
                    _logger.LogInformation("DeleteEmployeeRole\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        [HttpPost("SetEmployeePassword")]
        public async Task<bool> SetEmployeePassword([FromBody] JObject param)
        {
            bool result = false;

            var employeeId = param["EmployeeId"];
            var password = param["Password"];

            if (employeeId != null && password != null)
            {
                Employee employee = null;
                try
                {
                    int empId = Convert.ToInt32(employeeId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        employee = context.Employees.FirstOrDefault(o => o.Id == empId);
                        employee.Password = password.ToString();

                        int dbResult =  await context.SaveChangesAsync();
                        result = dbResult >= 0;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (employee != null && password != null)
                {
                    string message = "Password " + (result ? " Changed" : "Could Not Changed") + "  For Employee " + employee.Name + " " + employee.Surname;
                    await _dbLogger.logInfo(message, GetUserName());
                    _logger.LogInformation("SetEmployeeRole\tParam: " + JsonConvert.SerializeObject(param) + "\tResult: " + result);
                }
            }
            return result;
        }

        #endregion

        #region Location
        [HttpPost("GetLocationList")]
        public async Task<List<Location>> GetLocationList([FromBody] JObject param)
        {
            List<Location> list = new List<Location>();
            try
            {
                var employeeId = param["EmployeeId"];
                if(employeeId!=null)
                {
                    int empId = Convert.ToInt32(employeeId);
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        list = await (from m in context.Locations where m.EmployeeId == empId select m).ToListAsync();
                    }
                }
               
                _logger.LogInformation("GetLocationList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddLocation")]
        public async Task<bool> AddLocation([FromBody] Location location)
        {
            _logger.LogInformation("Location: " + JsonConvert.SerializeObject(location));
            bool result = false;
            if (location != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var dbResult = context.Locations.Add(location);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                if (location != null)
                {
                    string message = JsonConvert.SerializeObject(location) + (result ? " Added" : "Could Not Added");
                    _logger.LogInformation("AddLocation\tParam: " + JsonConvert.SerializeObject(location) + "\tResult: " + result);
                    await _dbLogger.logInfo(message, GetUserName());
                }
            }
            return result;
        }
        #endregion
    }
}
