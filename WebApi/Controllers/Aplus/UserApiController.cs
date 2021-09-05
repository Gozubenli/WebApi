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
using WebApi.UiModels;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly ILogger<AddressApiController> _logger;
        private CrmDbContext _db;
        public UserApiController(ILogger<AddressApiController> logger, CrmDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost("GetUserList")]
        public async Task<List<User>> GetUserList([FromBody] JObject param)
        {
            List<User> list = new List<User>();
            try
            {
                list = await (from m in _db.Users select m).ToListAsync();
                _logger.LogInformation("GetUserList Count:" + list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return list;
        }

        [HttpPost("AddUser")]
        public async Task<bool> AddUser([FromBody] User user)
        {
            _logger.LogInformation("User: " + JsonConvert.SerializeObject(user));
            bool result = false;
            if (user != null)
            {
                try
                {
                    user.CreatedDate = DateTime.UtcNow;
                    user.UpdateDate = DateTime.UtcNow;
                    var dbResult = _db.Users.Add(user);
                    await _db.SaveChangesAsync();
                    result = dbResult != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            _logger.LogInformation("AddUser Result:" + result);
            return result;
        }

        [HttpPost("UpdateUser")]
        public async Task<bool> UpdateUser([FromBody] User user)
        {
            _logger.LogInformation("UpdateUser: " + JsonConvert.SerializeObject(user));
            bool result = false;
            if (user != null)
            {
                try
                {
                    var existing = _db.Users.FirstOrDefault(o => o.Id == user.Id);
                    if (existing != null)
                    {
                        existing.Name = user.Name;
                        existing.SurName = user.SurName;
                        existing.UserName = user.UserName;
                        existing.Email = user.Email;
                        existing.Phone = user.Phone;
                        existing.Password = user.Password;
                        existing.Status = user.Status;
                        existing.UpdateDate = DateTime.UtcNow;
                        int dbResult = await _db.SaveChangesAsync();
                        result = dbResult > 0;
                    }
                    else
                    {
                        _logger.LogError("UpdateUser Not Found");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            _logger.LogInformation("UpdateUser Result:" + result);
            return result;
        }

        [HttpPost("DeleteUser")]
        public async Task<bool> DeleteUser([FromBody] User user)
        {
            _logger.LogInformation("DeleteUser: " + JsonConvert.SerializeObject(user));
            bool result = false;
            if (user != null)
            {
                try
                {
                    var existing = _db.Users.FirstOrDefault(o => o.Id == user.Id);
                    if (existing != null)
                    {
                        _db.Users.Remove(existing);
                        int dbResult = await _db.SaveChangesAsync();
                        result = dbResult > 0;
                    }
                    else
                    {
                        _logger.LogError("DeleteUser Not Found");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            _logger.LogInformation("DeleteUser Result:" + result);
            return result;
        }

        [HttpPost("Login")]
        public async Task<UiUserModel> Login([FromBody] JObject param)
        {
            var userName = param["UserName"];
            var password = param["Password"];

            UiUserModel result = null;
            try
            {
                if (userName != null && password != null)
                {
                    User user = await (from m in _db.Users where m.UserName == userName.ToString() && m.Password == password.ToString() select m).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        user.Password = "";
                        result = new UiUserModel();
                        result.User = user;
                        result.RoleList = await (from m in _db.Roles
                                                 join mn in _db.User_Roles on m.Id equals mn.RoleId
                                                 where mn.UserId == user.Id
                                                 select m).ToListAsync();
                        _logger.LogInformation("Login " + userName.ToString());
                    }
                    else
                    {
                        _logger.LogError("Login Error " + userName.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return result;
        }

        [HttpPost("TestData")]
        public async Task<bool> TestData([FromBody] JObject param)
        {
            try
            {
                _db.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Roles");
                _db.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Projects");
                _db.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Users");
                _db.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Customers");
                _db.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Address");
                _db.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Customer_Projects");



                Role role1 = new Role()
                {
                    Name = "Admin",
                    Description = "Administrator"
                };
                Role role2 = new Role()
                {
                    Name = "Standart",
                    Description = "Standart"
                };

                _db.Roles.Add(role1);
                _db.Roles.Add(role2);

                Project project1 = new Project()
                {
                    Name = "Project1",
                    Description = "Description1"
                };

                Project project2 = new Project()
                {
                    Name = "Project2",
                    Description = "Description2"
                };

                _db.Projects.Add(project1);
                _db.Projects.Add(project2);

                for (int i = 1; i < 11; i++)
                {
                    User user = new User()
                    {
                        Name = "Name" + i,
                        SurName = "SurName" + i,
                        Email = "email" + i + "@email.com",
                        Password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(i.ToString())),
                        Phone = "971000000" + i,
                        Status = 1,
                        UserName = "UserName" + i,
                    };
                    _db.Users.Add(user);

                    User_Role user_Role = new User_Role()
                    {
                        UserId = i,
                        RoleId = 1
                    };
                    _db.User_Roles.Add(user_Role);

                    Customer customer = new Customer()
                    {
                        Name = "Name" + i,
                        Surname = "Surname" + i,
                        Email = "email" + i + "@email.com",
                        Telephone = "971000000" + i,
                        UserName = "UserName" + i,
                        RecordBase = Utils.RecordBase.Crm
                    };

                    _db.Customers.Add(customer);

                    Address address1 = new Address()
                    {
                        Name = "Home",
                        City = "Dubai",
                        Country = "UAE",
                        CustomerId = i,
                        Detail = "Address Detail " + i
                    };

                    Address address2 = new Address()
                    {
                        Name = "Office",
                        City = "Dubai",
                        Country = "UAE",
                        CustomerId = i,
                        Detail = "Office Detail " + i
                    };
                    _db.Address.Add(address1);
                    _db.Address.Add(address2);                  

                    Customer_Project customer_Project1 = new Customer_Project() { 
                        ProjectId = 1, 
                        CustomerId = i, 
                    };

                    Customer_Project customer_Project2 = new Customer_Project()
                    {
                        ProjectId = 2,
                        CustomerId = i,
                    };

                    _db.Customer_Projects.Add(customer_Project1);
                    _db.Customer_Projects.Add(customer_Project2);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return true;
        }
    }
}
