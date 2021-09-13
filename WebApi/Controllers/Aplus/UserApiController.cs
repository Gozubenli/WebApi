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
using Microsoft.AspNetCore.Http;
using WebApi.Utils;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebApi.Aplus.Controllers
{
    [Route("aplus/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly ILogger<AddressApiController> _logger;        
        private readonly IDbContextFactory<CrmDbContext> _contextFactory;
        private DbLogger _dbLogger;
        public UserApiController(ILogger<AddressApiController> logger, IDbContextFactory<CrmDbContext> contextFactory)
        {
            _logger = logger;           
            _contextFactory = contextFactory;
            _dbLogger = new DbLogger(_contextFactory);
        }

        [HttpPost("GetUserList")]
        public async Task<List<User>> GetUserList([FromBody] JObject param)
        {
            List<User> list = new List<User>();
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    list = await (from m in context.Users select m).ToListAsync();
                }

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
            bool result = false;
            if (user != null)
            {
                try
                {
                    user.CreatedDate = DateTime.UtcNow;
                    user.UpdateDate = DateTime.UtcNow;
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var dbResult = context.Users.Add(user);
                        await context.SaveChangesAsync();
                        result = dbResult != null;
                    }                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "User " + user.Name + (result ? " Added" : "Could Not Added");
                _logger.LogInformation("AddUser\tParam: " + JsonConvert.SerializeObject(user) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
            _logger.LogInformation("AddUser Result:" + result);
            return result;
        }

        [HttpPost("UpdateUser")]
        public async Task<bool> UpdateUser([FromBody] User user)
        {
            bool result = false;
            if (user != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Users.FirstOrDefault(o => o.Id == user.Id);
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
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("UpdateUser Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "User " + user.Name + (result ? " Updated" : "Could Not Updated");
                _logger.LogInformation("UpdateUser\tParam: " + JsonConvert.SerializeObject(user) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
            return result;
        }

        [HttpPost("DeleteUser")]
        public async Task<bool> DeleteUser([FromBody] User user)
        {
            bool result = false;
            if (user != null)
            {
                try
                {
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        var existing = context.Users.FirstOrDefault(o => o.Id == user.Id);
                        if (existing != null)
                        {
                            context.Users.Remove(existing);
                            int dbResult = await context.SaveChangesAsync();
                            result = dbResult > 0;
                        }
                        else
                        {
                            _logger.LogError("DeleteUser Not Found");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                string message = "User " + user.Name + (result ? " Deleted" : "Could Not Deleted");
                _logger.LogInformation("DeleteUser\tParam: " + JsonConvert.SerializeObject(user) + "\tResult: " + result);
                await _dbLogger.logInfo(message, getUserName());
            }
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
                    User user = null;
                    using (var context = _contextFactory.CreateDbContext())
                    {
                        user = await (from m in context.Users where m.UserName == userName.ToString() && m.Password == password.ToString() select m).FirstOrDefaultAsync();
                    }

                    if (user != null)
                    {
                        result = new UiUserModel();
                        user.Password = "";
                        result.User = user;

                        using (var context = _contextFactory.CreateDbContext())
                        {
                            result.RoleList = await (from m in context.Roles
                                                     join mn in context.User_Roles on m.Id equals mn.RoleId
                                                     where mn.UserId == user.Id
                                                     select m).ToListAsync();
                        }
                        _logger.LogInformation("Login " + userName.ToString());

                        string message = "User " + userName + " Logged In";
                        _logger.LogInformation("Login\tParam: " + JsonConvert.SerializeObject(user) + "\tLogged In");
                        await _dbLogger.logInfo(message, getUserName());
                    }
                    else
                    {
                        string message = "User " + userName + " Could Not Logged In";
                        _logger.LogInformation("Login\tParam: " + JsonConvert.SerializeObject(param) + "\tCould Not Logged In");
                        await _dbLogger.logInfo(message, userName.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }

            return result;
        }

        [HttpPost("GetData")]
        public async Task<List<User>> GetData()
        {
            List<User> userList = new List<User>();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync("/users").Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                userList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            client.Dispose();
            return userList;
        }

        [HttpPost("TestData")]
        public async Task<bool> TestData([FromBody] JObject param)
        {
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Roles");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Projects");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Users");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Customers");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Address");
                    context.Database.ExecuteSqlRaw("TRUNCATE TABLE aplus.Customer_Projects");



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

                    context.Roles.Add(role1);
                    context.Roles.Add(role2);

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

                    context.Projects.Add(project1);
                    context.Projects.Add(project2);

                    var list = GetData().Result;
                    list.AddRange(GetData().Result);
                    list.AddRange(GetData().Result);
                    list.AddRange(GetData().Result);
                    list.AddRange(GetData().Result);
                    list.AddRange(GetData().Result);

                    for (int i = 1; i < list.Count; i++)
                    {
                        //User user = new User()
                        //{
                        //    Name = "Name" + i,
                        //    SurName = "SurName" + i,
                        //    Email = "email" + i + "@email.com",
                        //    Password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(i.ToString())),
                        //    Phone = "971000000" + i,
                        //    Status = 1,
                        //    UserName = "UserName" + i,
                        //};
                        list[i].Id = i;
                        list[i].UserName = "UserName" + i;
                        list[i].SurName = list[i].Name.Split(" ")[1];
                        list[i].Name = list[i].Name.Split(" ")[0];
                        list[i].Password = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(i.ToString()));
                        list[i].Status = 1;
                        context.Users.Add(list[i]);

                        User_Role user_Role = new User_Role()
                        {
                            UserId = i,
                            RoleId = 1
                        };
                        context.User_Roles.Add(user_Role);

                        Customer customer = new Customer()
                        {
                            Name = list[i].Name,
                            Surname = list[i].SurName,
                            Email = "email" + i + "@email.com",
                            Telephone = list[i].Phone,
                            UserName = "UserName" + i,
                            RecordBase = Utils.RecordBase.Crm
                        };

                        context.Customers.Add(customer);

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
                        context.Address.Add(address1);
                        context.Address.Add(address2);

                        Customer_Project customer_Project1 = new Customer_Project()
                        {
                            ProjectId = 1,
                            CustomerId = i,
                        };

                        Customer_Project customer_Project2 = new Customer_Project()
                        {
                            ProjectId = 2,
                            CustomerId = i,
                        };

                        context.Customer_Projects.Add(customer_Project1);
                        context.Customer_Projects.Add(customer_Project2);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return true;
        }
        private string getUserName()
        {
            return HttpContext.Session.GetString("UserName");
        }
    }


}
